interface ICoinObjective
{
    public Coin coinToUnlock { get; }
    public void UnlockCoin();
}
