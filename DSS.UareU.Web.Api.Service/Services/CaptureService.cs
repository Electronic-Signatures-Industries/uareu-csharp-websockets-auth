using DPUruNet;
using DSS.A2F.Fingerprint.Api.Shared;
using DSS.A2F.Fingerprint.Api.Shared.Mediatypes;
using DSS.UareU.Web.Api.Service.Models;
using MongoDB.Driver;
using Nancy.Responses;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Services
{
    public class CaptureService
    {
        public void RemoveCapture(string id)
        {
            var filter = Builders<FingerCapture>.Filter.Where(i => i.Id == id);
            var coll = FingerCapture.GetCollection();
            coll.DeleteOne(filter);

        }

        public Task<Nancy.Response> SaveCaptureAsync(byte[] fmd, byte[] compressedImage)
        {
            var model = SaveCapture(fmd, compressedImage);

            if (model == null)
            {
                return Task.FromResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "Invalid data"));
            } else
            {
                return Task.FromResult(ResponseMessageBuilder.BuildLocationResponse(Nancy.HttpStatusCode.Created, "api/v1/capture/" + model.Id));
            }
        }
        public FingerCapture SaveCapture(byte[] fmd, byte[] compressedImage)
        {
            // Decompress the image
            byte[] uncompressedData = DPUruNet.WSQ.UnCompressNIST(compressedImage, WSQ.IMAGE_FORMAT.DPFJ_FID_ANSI_381_2004);
            
            var fid = Importer.ImportFid(uncompressedData, Constants.Formats.Fid.ANSI);

            if (fid.ResultCode == Constants.ResultCode.DP_SUCCESS)
            {
                var imageView = fid.Data.Views.FirstOrDefault();
                // Image
                var img = BitmapConverter.CreateBitmap(imageView.RawImage, imageView.Width, imageView.Height);

                MemoryStream stream = new MemoryStream();
                img.Save(stream, ImageFormat.Jpeg);
                stream.Position = 0;

                var model = new FingerCapture
                {
                    FMD = Encryption.Encrypt(fmd),
                    Image = Encryption.Encrypt(stream.ToArray()),
                    WSQImage = Encryption.Encrypt(compressedImage),
                };

                var coll = FingerCapture.GetCollection();
                coll.InsertOne(model);
                return model;
            }
            else
            {
                return null;
            }
        }


        public Task GetCaptureImageAsync(string id, FindCaptureOptions options)
        {
            FingerCapture model = null;
            var filter = Builders<FingerCapture>.Filter.Where(i => i.Id == id);
            var coll = FingerCapture.GetCollection();
            model = coll.Find(filter).FirstOrDefault();

            if (model == null)
            {
                return Task.FromResult(ResponseMessageBuilder.BuildMessageResponse(Nancy.HttpStatusCode.BadRequest, "No capture found"));
            }

            // decrypt
            var image = Encryption.Decrypt(model.Image);

            MemoryStream stream = new MemoryStream();

            if (options.Extended)
            {
                var fmd = Encryption.DecryptToBase64(model.FMD);
                var wsq = Encryption.DecryptToBase64(model.WSQImage);

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
