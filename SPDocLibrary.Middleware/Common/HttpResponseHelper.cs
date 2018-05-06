using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace SPDocLibrary.Middleware.Common
{
    public static class HttpResponseHelper
    {
        public static HttpResponseException GetUnauthorizedResponseException(Exception ex, int authTokenLength = 0)
        {
            return GetUnauthorizedResponseException(ex.ToString(), authTokenLength);
        }

        public static HttpResponseException GetUnauthorizedResponseException(string errorMessage, int authTokenLength = 0)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Content = new StringContent(
                String.Format("Unauthorized: authTokenLength: {0}, error: {1}", authTokenLength, errorMessage)
                , Encoding.UTF8, "text/plain");
            return new HttpResponseException(response);
        }

        public static HttpResponseException GetExceptionResponse(HttpStatusCode statusCode, string message)
        {
            var response = new HttpResponseMessage(statusCode);
            response.Content = new StringContent(message, Encoding.UTF8, "text/plain");
            return new HttpResponseException(response);
        }

        public static HttpResponseException GetExceptionResponse(Exception e)
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            response.Content = new StringContent(String.Format("Exception: {0}. Stack Trace: {1}.", e.Message, e.StackTrace), Encoding.UTF8, "text/plain");
            return new HttpResponseException(response);
        }

        public static HttpResponseMessage GetOkResponse()
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent("Status-OK", Encoding.UTF8, "text/plain");
            return resp;
        }

        public static HttpResponseMessage GetErrorResponse()
        {
            var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            resp.Content = new StringContent("Status-Error", Encoding.UTF8, "text/plain");
            return resp;
        }
    }
}