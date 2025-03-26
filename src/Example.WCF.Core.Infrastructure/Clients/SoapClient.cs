using System.Text;
using Example.WCF.Core.Domain.Services;

namespace Example.WCF.Core.Infrastructure.Clients;

public class SoapClient(AppSettingsService appSettingsService)
{
	private readonly string? soapAction = appSettingsService.GetAppSettings().SoapAction;
	public async Task<string> SendSoapRequest(string soapRequest, string soapEndpoint)
	{
		using HttpClient client = new();
		HttpRequestMessage request = new(HttpMethod.Post, soapEndpoint);

		var content = new StringContent(soapRequest, Encoding.UTF8, "application/soap+xml");
		// Override Content-Type to include SOAP action
		content.Headers.ContentType!.Parameters.Clear(); // Remove extra parameters
		content.Headers.ContentType!.Parameters.Add(new System.Net.Http.Headers.NameValueHeaderValue("action", $"\"{soapAction}\""));

		request.Content = content;

		HttpResponseMessage response = await client.SendAsync(request);
		string responseContent = await response.Content.ReadAsStringAsync();

		return responseContent;
	}
}
