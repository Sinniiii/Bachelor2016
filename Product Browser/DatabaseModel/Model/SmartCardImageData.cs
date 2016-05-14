namespace DatabaseModel.Model
{
    public class SmartCardImageData
    {
        public int Id { get; private set; }

        public byte[] Data { get; private set; }

        public void SetData(byte[] data)
        {
            Data = data;
        }

        public SmartCardImageData(byte[] data)
        {
            Data = data;
        }

        // For Entity framework
        private SmartCardImageData() { }
    }
}
