namespace FGJ.LiarDice
{
    public readonly struct BidInputResult
    {
        public readonly bool IsValid;
        public readonly Bid Bid;
        public readonly string ErrorMessage;

        private BidInputResult(bool isValid, Bid bid, string errorMessage)
        {
            IsValid = isValid;
            Bid = bid;
            ErrorMessage = errorMessage;
        }

        public static BidInputResult Success(Bid bid) => new BidInputResult(true, bid, null);
        public static BidInputResult Fail(string message) => new BidInputResult(false, default, message);
    }

    public sealed class BidInputValidator
    {
        private readonly ILiarDiceRules _rules;

        public BidInputValidator(ILiarDiceRules rules)
        {
            _rules = rules ?? throw new System.ArgumentNullException(nameof(rules));
        }

        public BidInputResult Validate(string quantityText, string faceText, Bid? currentBid, int totalDice)
        {
            quantityText = quantityText?.Trim();
            faceText = faceText?.Trim();

            if (string.IsNullOrEmpty(quantityText))
                return BidInputResult.Fail("請輸入數量。");
            if (string.IsNullOrEmpty(faceText))
                return BidInputResult.Fail("請輸入點數。");
            if (!int.TryParse(quantityText, out var quantity))
                return BidInputResult.Fail("數量必須是整數。");
            if (!int.TryParse(faceText, out var face))
                return BidInputResult.Fail("點數必須是整數。");

            var bid = new Bid(quantity, face);
            switch (_rules.Validate(currentBid, bid, totalDice))
            {
                case BidValidation.FaceOutOfRange:
                    return BidInputResult.Fail($"點數必須介於 {LiarDiceRules.MinFace}～{LiarDiceRules.MaxFace}。");
                case BidValidation.QuantityTooLow:
                    return BidInputResult.Fail("數量至少要 1。");
                case BidValidation.QuantityTooHigh:
                    return BidInputResult.Fail($"數量不能超過場上骰子總數（{totalDice}）。");
                case BidValidation.NotHigher:
                    return BidInputResult.Fail($"必須比目前的「{currentBid.Value}」大：數量更多，或同數量點數更大。");
                default:
                    return BidInputResult.Success(bid);
            }
        }
    }
}
