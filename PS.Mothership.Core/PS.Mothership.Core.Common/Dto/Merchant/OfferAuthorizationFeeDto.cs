namespace PS.Mothership.Core.Common.Dto.Merchant
{
    public class OfferAuthorizationFeeDto
    {
        public long AuthorisationFeeKey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
    }
}