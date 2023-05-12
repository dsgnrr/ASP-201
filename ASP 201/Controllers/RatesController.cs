using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_201.Controllers
{
    [Route("api/rates")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        [HttpGet]
        public object Get([FromQuery] String data) //прийом данних
        {
            return new { result = $"Запит оброблено методом GET і прийнято дані {data}" };
        }
        [HttpPost]
        public object Post([FromBody] BodyData bodyData)
        {
            return new {
                result = $"Запит оброблено методом POST і прийнято дані {bodyData.Data}"
            };
        }
        public object Default()
        {
            switch (HttpContext.Request.Method)
            {
                case "LINK": return Link();
                default: throw new NotImplementedException();
            }
           
        }
        private object Link()
        {
            return new
            {
                result = $"Запит оброблено методом {HttpContext.Request.Method} і прийнято дані --"
            };
        }
    }
    public class BodyData
    {
        public String Data { get; set; } = null!;
    }
}
