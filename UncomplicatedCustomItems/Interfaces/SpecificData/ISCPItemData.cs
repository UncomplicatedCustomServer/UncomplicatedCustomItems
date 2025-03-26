namespace UncomplicatedCustomItems.Interfaces.SpecificData
{
    public interface ISCPItemData : IData
    {

    }
    public interface ISCP500Data : ISCPItemData
    {
        
    }

    public interface ISCP207Data : ISCPItemData
    {
        
    }
    public interface ISCP018Data : ISCPItemData
    {
        public abstract float FriendlyFireTime { get; set; }

        public abstract float FuseTime { get; set; }
    }

    public interface ISCP268Data : ISCPItemData
    {
        
    }
    public interface ISCP330Data : ISCPItemData
    {
        
    }

    public interface ISCP2176Data : ISCPItemData
    {
        public abstract float FuseTime { get; set; }
    }
    public interface ISCP244aData : ISCPItemData
    {
        
    }
    public interface ISCP244bData : ISCPItemData
    {
        
    }

    public interface ISCP1853Data : ISCPItemData
    {
        
    }
    public interface ISCP1576Data : ISCPItemData
    {
        
    }
    public interface ISCP1344Data : ISCPItemData
    {
        
    }
}