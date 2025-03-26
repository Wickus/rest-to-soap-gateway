using Example.WCF.Core.Application.Services;
using Example.WCF.Core.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Example.WCF.Core.Api.Controllers
{
	[ApiController]
	public class RestToSoapController(RestToSoapService restToSoapService) : ControllerBase
	{
		private readonly RestToSoapService _restToSoapService = restToSoapService;

		[HttpPost]
		[Route("/api/v1/consumer-submit")]
		public async Task<ActionResult> Post([FromBody] SoapRequest request)
		{
			try
			{
				string response = await _restToSoapService.ConvertAndSend(request);

				return new ContentResult
				{
					Content = response,
					ContentType = "application/soap+xml",
					StatusCode = 200
				};
			}
			catch
			{
				return BadRequest();
			}
		}
	}
}
