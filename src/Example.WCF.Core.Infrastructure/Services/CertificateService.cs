using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Example.WCF.Core.Domain.Models;
using Example.WCF.Core.Domain.Services;

namespace Example.WCF.Core.Infrastructure.Services;

public class CertificateService
{
	private readonly X509Certificate2? _certificate;
	private readonly AppSettings _appSettings;

	public CertificateService(AppSettingsService appSettingsService)
	{
		_appSettings = appSettingsService.GetAppSettings();
		_certificate = LoadCertificate();
	}

	private X509Certificate2? LoadCertificate()
	{
		string? thumbprint = _appSettings.CertThumbprint?.Replace(" ", "").ToUpperInvariant();

		if (OperatingSystem.IsWindows())
		{
			using X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
			store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

			X509Certificate2? cert = store.Certificates
				.OfType<X509Certificate2>()
				.FirstOrDefault(c => c.Thumbprint?.ToUpperInvariant() == thumbprint);

			store.Close();

			return cert ?? GetCertFromFileLocation();
		}

		return GetCertFromFileLocation();
	}

	private X509Certificate2? GetCertFromFileLocation()
	{
		if (!string.IsNullOrEmpty(_appSettings.CertStorePath) && File.Exists(_appSettings.CertStorePath))
		{
			return new X509Certificate2(_appSettings.CertStorePath, _appSettings.CertStorePassword ?? "");
		}

		return null;
	}

	public string GetBinarySecurityToken() =>
		_certificate != null ? Convert.ToBase64String(_certificate.RawData) : string.Empty;

	public string GetSubjectKeyIdentifier()
	{
		if (_certificate == null) return string.Empty;

		byte[] publicKeyBytes = _certificate.GetPublicKey();
		byte[] skiBytes = SHA1.HashData(publicKeyBytes);
		return Convert.ToBase64String(skiBytes);
	}

	public RSA? GetPrivateKey() => _certificate?.GetRSAPrivateKey();

	public RSA? GetPublicKey() => _certificate?.GetRSAPublicKey();
}
