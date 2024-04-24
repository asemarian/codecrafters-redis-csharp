class StoredValue
{
    public string Value { get; set; }
    public long Expiry { get; set; }

    public StoredValue(string value, long ttl)
    {
        Value = value;
        Expiry = ttl;
    }

}