using System.Xml;
using Example.WCF.Core.Domain.Constants;
using Example.WCF.Core.Domain.Enums;
using Example.WCF.Core.Domain.Models;
using Example.WCF.Core.Infrastructure.Models;
using Newtonsoft.Json;

namespace Example.WCF.Core.Infrastructure.Services;

public class SoapBodyService()
{
	public static XmlElement CreateBody(XmlDocument xmlDocument, SoapRequest soapRequest, SoapElementIds soapElementIds)
	{
		string submitMessageString = JsonConvert.SerializeObject(soapRequest.Body);

		return soapRequest.Header.ProductId switch
		{
			(byte)SecureXProduct.SPX => CreateSPXBody(xmlDocument, submitMessageString),
			_ => xmlDocument.CreateElement("soap", "Body", SoapNamespaces.SoapEnvelope),
		};
	}

	private static XmlElement CreateSPXBody(XmlDocument xmlDocument, string submitMessageString)
	{
		IVXConsumerSubmitMessage? consumerSubmitMessage = JsonConvert.DeserializeObject<IVXConsumerSubmitMessage>(submitMessageString);

		XmlElement bodyElement = xmlDocument.CreateElement("soap", "Body", SoapNamespaces.SoapEnvelope);
		XmlElement secureXBodyElement = xmlDocument.CreateElement("Body");

		if (consumerSubmitMessage?.Fields.Length == 0)
		{
			bodyElement.AppendChild(secureXBodyElement);
			return bodyElement;
		}

		secureXBodyElement.SetAttribute("xmlns:i", SoapNamespaces.MessageSchema);
		secureXBodyElement.SetAttribute("xmlns", SecureXNamespaces.ServiceContract);
		secureXBodyElement.SetAttribute("type", SoapNamespaces.MessageSchema, "a:" + consumerSubmitMessage?.GetType().Name);
		secureXBodyElement.SetAttribute("xmlns:a", SecureXNamespaces.ProductContract[(byte)SecureXProduct.SPX]);

		XmlElement felidsElement = CreateSecureXBodyFields(xmlDocument, consumerSubmitMessage?.Fields ?? [], SecureXNamespaces.ProductContract[(byte)SecureXProduct.SPX]);

		secureXBodyElement.AppendChild(felidsElement);
		bodyElement.AppendChild(secureXBodyElement);

		return bodyElement;
	}

	private static XmlElement CreateSecureXBodyFields(XmlDocument xmlDocument, KeyValuePair<string, string>[] secureXKeyValuePair, string productContract)
	{
		XmlElement fieldsElement = xmlDocument.CreateElement("a", "Fields", productContract);

		fieldsElement.SetAttribute("xmlns:b", SecureXNamespaces.CommonContract);
		fieldsElement.SetAttribute("xmlns:a", SecureXNamespaces.ProductContract[(byte)SecureXProduct.SPX]);

		if (secureXKeyValuePair.Length == 0)
		{
			fieldsElement.SetAttribute("nil", SoapNamespaces.MessageSchema, "true");
		}

		foreach (KeyValuePair<string, string> keyValuePair in secureXKeyValuePair)
		{
			XmlElement keyValuePairElement = xmlDocument.CreateElement("b", "KeyValuePair", SecureXNamespaces.CommonContract);
			XmlElement keyElement = xmlDocument.CreateElement("b", "Key", SecureXNamespaces.CommonContract);
			XmlElement valueElement = xmlDocument.CreateElement("b", "Value", SecureXNamespaces.CommonContract);

			keyElement.InnerText = keyValuePair.Key;
			valueElement.InnerText = keyValuePair.Value;

			keyValuePairElement.AppendChild(keyElement);
			keyValuePairElement.AppendChild(valueElement);
			fieldsElement.AppendChild(keyValuePairElement);
		}

		return fieldsElement;
	}
}
