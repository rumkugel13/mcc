namespace mcc
{
    internal class Optimizer
    {
        private ASTNode rootNode;
        private Optimizations optimizations;
        public OptimizationStats Stats;

        public enum Optimizations
        {
            None = 0,
            ConstantFolding = 1,
        }

        public struct OptimizationStats
        {
            public uint Count;
            public readonly bool AtLeastOneOptimization => Count > 0;
        }

        public Optimizer(ASTNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public void OptimizeAST(Optimizations optimizations)
        {
            this.optimizations = optimizations;
            Optimize(rootNode);
        }

        private void Optimize(ASTNode node)
        {
            switch (node)
            {
                case ASTNoExpressionNode: break;
                case ASTNoStatementNode: break;
                case ASTProgramNode program: OptimizeProgram(program); break;
                case ASTFunctionNode function: OptimizeFunction(function); break;
                case ASTReturnNode ret: OptimizeReturn(ret); break;
                case ASTConstantNode constant: OptimizeConstant(constant); break;
                case ASTUnaryOpNode unOp: OptimizeUnaryOp(unOp); break;
                case ASTBinaryOpNode binOp: OptimizeBinaryOp(binOp); break;
                case ASTExpressionNode exp: OptimizeExpression(exp); break;
                case ASTDeclarationNode dec: OptimizeDeclaration(dec); break;
                case ASTAssignNode assign: OptimizeAssign(assign); break;
                case ASTVariableNode variable: OptimizeVariable(variable); break;
                case ASTConditionNode cond: OptimizeCondition(cond); break;
                case ASTConditionalExpressionNode condEx: OptimizeConditionalExpression(condEx); break;
                case ASTCompundNode comp: OptimizeCompound(comp); break;
                case ASTWhileNode whil: OptimizeWhile(whil); break;
                case ASTDoWhileNode doWhil: OptimizeDoWhile(doWhil); break;
                case ASTForNode fo: OptimizeFor(fo); break;
                case ASTForDeclarationNode forDecl: OptimizeForDeclaration(forDecl); break;
                case ASTBreakNode br: OptimizeBreak(br); break;
                case ASTContinueNode con: OptimizeContinue(con); break;
                case ASTFunctionCallNode funCall: OptimizeFunctionCall(funCall); break;
                default: throw new NotImplementedException("Unkown ASTNode type: " + node.GetType());
            }
        }

        private void OptimizeFunctionCall(ASTFunctionCallNode funCall)
        {
            foreach (var arg in funCall.Arguments)
            {
                Optimize(arg);
            }
        }

        private void OptimizeContinue(ASTContinueNode con)
        {
        }

        private void OptimizeBreak(ASTBreakNode br)
        {
        }

        private void OptimizeForDeclaration(ASTForDeclarationNode forDecl)
        {
            PushScope();
            Optimize(forDecl.Declaration);
            OptimizeAbstractExpression(ref forDecl.Condition);
            PushScope();
            Optimize(forDecl.Statement);
            PopScope();
            OptimizeAbstractExpression(ref forDecl.Post);
            PopScope();
        }

        private void OptimizeFor(ASTForNode fo)
        {
            PushScope();
            OptimizeAbstractExpression(ref fo.Init);
            OptimizeAbstractExpression(ref fo.Condition);
            PushScope();
            Optimize(fo.Statement);
            PopScope();
            OptimizeAbstractExpression(ref fo.Post);
            PopScope();
        }

        private void OptimizeDoWhile(ASTDoWhileNode doWhil)
        {
            PushScope();
            Optimize(doWhil.Statement);
            PopScope();
            OptimizeAbstractExpression(ref doWhil.Expression);
        }

        private void OptimizeWhile(ASTWhileNode whil)
        {
            OptimizeAbstractExpression(ref whil.Expression);
            PushScope();
            Optimize(whil.Statement);
            PopScope();
        }

        private void PushScope()
        {
        }

        private void PopScope()
        {
        }

        private void OptimizeCompound(ASTCompundNode comp)
        {
            PushScope();

            foreach (var blockItem in comp.BlockItems)
            {
                Optimize(blockItem);
            }

            PopScope();
        }

        private void OptimizeConditionalExpression(ASTConditionalExpressionNode condEx)
        {
            OptimizeAbstractExpression(ref condEx.Condition);
            OptimizeAbstractExpression(ref condEx.IfBranch);
            OptimizeAbstractExpression(ref condEx.ElseBranch);
        }

        private void OptimizeCondition(ASTConditionNode cond)
        {
            OptimizeAbstractExpression(ref cond.Condition);
            Optimize(cond.IfBranch);
            if (cond.ElseBranch is not ASTNoStatementNode)
            {
                Optimize(cond.ElseBranch);
            }
        }

        private void OptimizeVariable(ASTVariableNode variable)
        {
        }

        private void OptimizeAssign(ASTAssignNode assign)
        {
            OptimizeAbstractExpression(ref assign.Expression);
        }

        private void OptimizeDeclaration(ASTDeclarationNode dec)
        {
            if (dec.IsGlobal)
            {
                OptimizeGlobalDeclaration(dec);
                return;
            }

            if (dec.Initializer is not ASTNoExpressionNode)
            {
                OptimizeAbstractExpression(ref dec.Initializer);
            }
        }

        private void OptimizeGlobalDeclaration(ASTDeclarationNode dec)
        {
            if (dec.Initializer is not ASTNoExpressionNode)
            {
                if (!dec.Initializer.IsConstantExpression)
                {
                    OptimizeAbstractExpression(ref dec.Initializer);
                }
            }
        }

        private void OptimizeExpression(ASTExpressionNode exp)
        {
            OptimizeAbstractExpression(ref exp.Expression);
        }

        private void OptimizeAbstractExpression(ref ASTAbstractExpressionNode node)
        {
            if (optimizations.HasFlag(Optimizations.ConstantFolding) && node.IsConstantExpression)
            {
                node = new ASTConstantNode(Evaluator.Evaluate(node));
                Stats.Count++;
            }
            else
            {
                Optimize(node);
            }
        }

        private void OptimizeBinaryOp(ASTBinaryOpNode binOp)
        {
            OptimizeAbstractExpression(ref binOp.ExpressionLeft);
            OptimizeAbstractExpression(ref binOp.ExpressionRight);
        }

        private void OptimizeUnaryOp(ASTUnaryOpNode unOp)
        {
            OptimizeAbstractExpression(ref unOp.Expression);
        }

        private void OptimizeConstant(ASTConstantNode constant)
        {
        }

        private void OptimizeReturn(ASTReturnNode ret)
        {
            OptimizeAbstractExpression(ref ret.Expression);
        }

        private void OptimizeFunction(ASTFunctionNode function)
        {
            foreach (var blockItem in function.BlockItems)
            {
                Optimize(blockItem);
            }
        }

        private void OptimizeProgram(ASTProgramNode program)
        {
            foreach (var topLevelItem in program.TopLevelItems)
            {
                Optimize(topLevelItem);
            }
        }
    }
}