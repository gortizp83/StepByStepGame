using System;

namespace Microsoft.PartnerCatalyst.IntelligentHeadset
{
    class IHSUuids
    {
		// DeviceInfoService (adopted service)
		public const ushort DeviceInfoServiceId = 0x180A;
		public const ushort SystemId = 0x2A23;
		public const ushort ModelNumber = 0x2A24;
		public const ushort SerialNumber = 0x2A25;
        public const ushort FirmwareRevision = 0x2A26;
        public const ushort HardwareRevision = 0x2A27;
		public const ushort SoftwareRevision = 0x2A28;
		public const ushort ManufacturerName = 0x2A29;
		
		// BattService (adopted service)
		public const ushort BatteryServiceId =  0x180F;
		public const ushort BatteryLevel = 0x2A19;

        // GnSystemService (adopted service)
        public const string GnSystemService = "8f8fe645-9e32-4e12-9150-f6b488f6b5aa";
		public const string Nonce =  "96593bf7-459b-422a-8808-a17f678a5bec";
		public const string Key = "9fc790c1-c7bb-49bf-8d7a-965267b5802f";	
		public const string Features = "35f59ce8-f152-41cd-ac5f-8ce6d7e249f8";	
		public const string Configuration = "f38db2f6-ade3-4805-9b4f-00530e6452bb";	
		public const string FotaStatus = "736491b1-1868-47e1-a30abc5f4b3b975b";	
		public const string Statistics = "95e0f0aa-b390-46d5-85f3-01e2aaf4c10a";

        // GnImuService
        public const string GnImuService = "7ca251df-137b-41b2-9169-1c0215bea6de";
        public const string UpdateInterval = "652949ce-a54e-40b2-b27b-407d944e14e3";
		public const string MagVector =  "32d9c336-722b-4ec5-998f-4f7dcf08f465";
		public const string AccVector = "e1f1e3bd-9672-4213-948d-206c4fa9820f";	
		public const string Calibration = "ad6486a9-bceb-4ebd-9ca3-48b9a9675be5";	
		public const string ComboHpr = "919d5add-298f-4431-acf9-9f67275f1455";	
		public const string MagParams = "760d93e8-476d-4d7c-a894-b95112d16aea";	
		public const string MagDistParams = "e08b0483-63eb-4920-86d5-c1e2b5ae6e72";
		
		// GnGpsService
		public const string GnGpsService = "52466e96-a001-425a-96b6-7d5795a1ea08";
		public const string AllInOne = "ca00ace5-1f61-41c2-82e3-a4d4fe416d31";
		public const string Rtcm = "7b5558b1-075e-4b37-b773-fc48773ba9b4";

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uuid16Bit"></param>
        /// <returns></returns>
        public static Guid CreateGuidfromWellKnownUUID(int uuid16Bit)
        {
            string uuidString = string.Format("0000{0,4:X}-0000-1000-8000-00805f9b34fb", uuid16Bit);

            if (uuid16Bit >= 0 && uuid16Bit < 0xFFFF)
            {
                return Guid.Parse(uuidString);
            }

            return Guid.Empty;
        }
    }
}
