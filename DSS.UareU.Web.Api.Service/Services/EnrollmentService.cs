using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSS.UareU.Web.Api.Service.Services
{
    public class EnrollmentService
    {
        // public Task<Nancy.Response> EnrollUser(List<string> tags) {
        //     // Enrollment.CreateEnrollmentFmd

        //     // save to db
        //     return null; // return FMD    
        // }

        public Task<Nancy.Response> EnrollImageAsFMD(List<string> tags, byte[] image, int width, int height)
        {
            // FeatureExtraction.CreateFmdFromFid
            /*
                        var id = Guid.NewGuid().ToString();
                        var img = CreateBitmap(image, width, height);
                        img.Save(id + ".jpg");
            */
            // save to db
            return null; // return id             
        }

    }
}
