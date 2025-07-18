namespace Neptune.NsPay.HttpExtensions.Crypto.Models
{
    internal class EthereumAPIResponse<T>
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
    }
}