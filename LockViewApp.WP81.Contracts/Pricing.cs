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
        public static double CalculateDrainPerRequest(LockViewRequestMetadata metaData, InterestGathering.RequestMetaData providerMetaData)
        {
           return (((metaData.ImageBytesPerRequest + providerMetaData.BytePerRequest) / (1024.0 * 1024 * 1024)) * Pricing.TrafficPricePerGB +
           (providerMetaData.TypicalComputationInSec / 3600.0) * Pricing.ComputationPricePerHour);
        }
    }
}
