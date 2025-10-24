namespace MyPortal.Common.Options
{
    public class CertificateOptions
    {
        public CertRef Signing { get; set; } = new();
        public CertRef Encryption { get; set; } = new();

        public sealed class CertRef
        {
            public StoreRef? Store { get; set; } = new();
            public PfxRef? Pfx { get; set; } = new();
        }

        public sealed class StoreRef
        {
            public string? Thumbprint { get; set; }
            public string StoreName { get; set; } = "My";
            public string StoreLocation { get; set; } = "LocalMachine";
        }

        public sealed class PfxRef
        {
            public string? Path { get; set; }
            public string? Password { get; set; }
        }
    }
}
