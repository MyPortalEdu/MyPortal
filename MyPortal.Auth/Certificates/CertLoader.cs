using System.Security.Cryptography.X509Certificates;
using MyPortal.Common.Options;

namespace MyPortal.Auth.Certificates
{
    public static class CertLoader
    {
        public static X509Certificate2 Load(CertificateOptions.CertRef r)
        {
            // Prefer Store if configured; else PFX.
            if (r.Store is { Thumbprint: { Length: > 0 } })
                return LoadFromStore(r.Store);
            if (r.Pfx is { Path: { Length: > 0 } })
                return LoadFromPfx(r.Pfx);

            throw new InvalidOperationException("Neither Store nor PFX cert config provided.");
        }

        private static X509Certificate2 LoadFromStore(CertificateOptions.StoreRef s)
        {
            if (string.IsNullOrWhiteSpace(s.Thumbprint))
                throw new InvalidOperationException("Thumbprint not provided.");

            var thumb = NormalizeThumbprint(s.Thumbprint);
            var storeLoc = Enum.Parse<StoreLocation>(s.StoreLocation, ignoreCase: true);
            var storeName = Enum.Parse<StoreName>(s.StoreName, ignoreCase: true);

            using var store = new X509Store(storeName, storeLoc);
            store.Open(OpenFlags.ReadOnly);
            var matches = store.Certificates.Find(
                X509FindType.FindByThumbprint, thumb, validOnly: false);

            var cert = matches.FirstOrDefault();

            if (cert is null)
            {
                throw new InvalidOperationException($"Certificate with thumbprint {thumb} not found in {storeLoc}\\{storeName}.");
            }

            return cert;
        }

        private static X509Certificate2 LoadFromPfx(CertificateOptions.PfxRef p)
        {
            if (string.IsNullOrWhiteSpace(p.Path))
                throw new InvalidOperationException("PFX path not provided.");
            // Password can be null/empty if PFX has no password.
            return new X509Certificate2(
                p.Path,
                p.Password,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);
        }

        private static string NormalizeThumbprint(string tp)
            => new string(tp.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}
