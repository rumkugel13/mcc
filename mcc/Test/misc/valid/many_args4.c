int eleven(int one, int two, int three, int four, int five, int six, int seven, int eight, int nine, int ten, int eleven)
{
    return one + two + three + four + five + six + seven + eight + nine + ten + eleven;
}

int main()
{
    return eleven(1, 2 + eleven(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11), 3, 4, 5, 6, 7, 8, 9, 10, 11);
}