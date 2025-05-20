interface ISaveable
{
    public bool CanSave { get; }
    public void Save();
    public void Load();
}
