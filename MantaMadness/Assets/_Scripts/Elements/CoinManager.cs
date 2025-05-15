using System;

public class CoinManager
{
    private static CoinManager _instance;
    public static CoinManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CoinManager();
            }

            return _instance;
        }

        set => _instance = value;
    }

    private CoinManager() { }

    private int pickupCoinCount;
    public Action<int> coinPickedUp;
    
    public void PickupCoin()
    {
        pickupCoinCount++;
        coinPickedUp?.Invoke(pickupCoinCount);
    }
}
