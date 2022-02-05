namespace Snail.Logic;

internal class Created : IState
{
    private readonly int[][] _array;

    public Created(int[][] array) => _array = array;

    public bool IsEndOfSnail => false;

    public int Current =>
        throw new InvalidOperationException("It is not allowed to access Current before MoveNext() has been called.");

    public IState MoveNext()
    {
        if (_array.Length > 0)
        {
            return new RightMovement(_array);
        }

        return new EndOfSnail();
    }
}