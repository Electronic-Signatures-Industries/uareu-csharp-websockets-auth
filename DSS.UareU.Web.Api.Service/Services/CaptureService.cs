using DSS.UareU.Web.Api.Service.Models;
using DSS.UareU.Web.Api.Shared;
using DSS.UareU.Web.Api.Shared.Mediatypes;
using MongoDB.Driver;
using Nancy.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Services
{
    public class CaptureService
    {
        Nancy.Response BuildMessageResponse(Nancy.HttpStatusCode code, string message)
        {
            var m = Encoding.UTF8.GetBytes(message);
            return new Nancy.Response
            {
                StatusCode = code,
                Contents = a => a.Write(m, 0, m.Length),
            };
        }

        public Task GetCaptureImageAsync(string id, FindCaptureOptions options)
        {
            FingerCapture model = null;
            var filter = Builders<FingerCapture>.Filter.Where(i => i.Id == id);
            var coll = FingerCapture.GetCollection();
            model = coll.Find(filter).FirstOrDefault();

            if (model == null)
            {
                return Task.FromResult(BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No capture found"));
            }

            // decrypt
            var image = Encryption.Decrypt(model.Image);

            MemoryStream stream = new MemoryStream();

            if (options.Extended)
            {
                var fmd = Convert.ToBase64String(model.FMD);
                var wsq = Convert.ToBase64String(model.WSQImage);

                return Task.FromResult(new CaptureResponseMediaType { Fmd = fmd, Wsq = wsq });
            }
            else
            {
                stream.Write(image, 0, image.Length);
                stream.Position = 0;
                var resp = new StreamResponse(() => stream, Nancy.MimeTypes.GetMimeType(id + ".jpg"));
                return Task.FromResult<Nancy.Response>(resp);
            }

        }


    }
}
