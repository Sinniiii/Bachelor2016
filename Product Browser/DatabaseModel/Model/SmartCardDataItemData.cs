﻿namespace DatabaseModel.Model
{
    public class SmartCardDataItemData
    {
        public int Id { get; private set; }

        public byte[] Data { get; private set; }

        public void SetData(byte[] data)
        {
            Data = data;
        }

        public SmartCardDataItemData(byte[] data)
        {
            Data = data;
        }

        // For Entity framework
        private SmartCardDataItemData() { }
    }
}
