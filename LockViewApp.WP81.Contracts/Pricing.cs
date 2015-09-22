using InfoViewApp.WP81.InterestGathering;
using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoViewApp.WP81
{
    public class Pricing
    {
        public const double ComputationPricePerHour = 0.4;
        public const double TrafficPricePerGB = 0.185;
        public static double CalculateDrainPerRequest(LockViewRequestMetadata metaData, RequestMetaData providerMetaData)
        {
            double multiplier = 1;
            //if (DeviceStatus.DeviceTotalMemory >> 29 < 1) multiplier = 1.5;//pay extra money for inbound.
            return (((metaData.ImageBytesPerRequest * multiplier + providerMetaData.BytePerRequest) / (1024.0 * 1024 * 1024)) * TrafficPricePerGB +
            (providerMetaData.TypicalComputationInSec / 3600.0) * ComputationPricePerHour);
        }
    }
}
