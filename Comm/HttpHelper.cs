using DataModel;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Communicator
{
	public class HttpHelper
	{
		public static List<ProcessStatusEvent> GetDataFromWebAPI()
		{
			string html = string.Empty;
			string url = @"http://localhost:49570/api/simulator";

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.AutomaticDecompression = DecompressionMethods.GZip;

			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				html = reader.ReadToEnd();
			}

			var userData = JsonConvert.DeserializeObject<List<ProcessStatusEvent>>(html);

			return userData;
		}

		public static void PostDataToWebAPI(ProcessStatusEvent PostData)
		{
			// Create a request using a URL that can receive a post.   
			WebRequest request = WebRequest.Create("http://localhost:49570/api/simulator");
			// Set the Method property of the request to POST.  
			request.Method = "POST";

			// Create POST data and convert it to a byte array.  
			string postData = JsonConvert.SerializeObject(PostData);
			byte[] byteArray = Encoding.UTF8.GetBytes(postData);

			// Set the ContentType property of the WebRequest.  
			request.ContentType = "application/json";
			// Set the ContentLength property of the WebRequest.  
			request.ContentLength = byteArray.Length;

			// Get the request stream.  
			Stream dataStream = request.GetRequestStream();
			// Write the data to the request stream.  
			dataStream.Write(byteArray, 0, byteArray.Length);
			// Close the Stream object.  
			dataStream.Close();

			// Get the response.  
			WebResponse response = request.GetResponse();
			// Display the status.  
			//Console.WriteLine(((HttpWebResponse)response).StatusDescription);

			// Get the stream containing content returned by the server.  
			// The using block ensures the stream is automatically closed.
			using (dataStream = response.GetResponseStream())
			{
				// Open the stream using a StreamReader for easy access.  
				StreamReader reader = new StreamReader(dataStream);
				// Read the content.  
				string responseFromServer = reader.ReadToEnd();
				// Display the content.  
				//Console.WriteLine(responseFromServer);
			}

			// Close the response.  
			response.Close();
		}
	}
}
