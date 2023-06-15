using ASP_201.Data;
using ASP_201.Data.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_201.Controllers
{
    [Route("api/rates")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly DataContext dataContext;

        public RatesController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        [HttpGet]
        public object Get([FromQuery] String data) //прийом данних
        {
            return new { result = $"Запит оброблено методом GET і прийнято дані {data}" };
        }
        [HttpPost]
        public object Post([FromBody] BodyData bodyData)
        {
            int statusCode;
            String result;

            if (bodyData == null
                || bodyData.Data == null
                || bodyData.ItemId == null
                || bodyData.UserId == null)
            {
                statusCode = StatusCodes.Status400BadRequest;
                result = $"Не всі дані передано: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
            }
            else
            {
                try
                {
                    Guid itemId = Guid.Parse(bodyData.ItemId);
                    Guid userId = Guid.Parse(bodyData.UserId);
                    int rating = Convert.ToInt32(bodyData.Data);
                    Rate? rate = dataContext.Rates.FirstOrDefault(r => r.UserId == userId && r.ItemId == itemId);
                    if (rate is not null)
                    {
                        if (rate.Rating == rating)
                        {
                            statusCode = StatusCodes.Status406NotAcceptable;
                            result = $"Дані вже наявні: ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                        }
                        else
                        {
                            // дані наявні, але з іншою оцінкою -- міняємо оцінку
                            rate.Rating = rating;
                            dataContext.SaveChanges();
                            statusCode = StatusCodes.Status202Accepted;
                            result = $"Дані оновлено: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                        }
                    }
                    else
                    {
                        dataContext.Rates.Add(new()
                        {
                            ItemId = itemId,
                            UserId = userId,
                            Rating = rating
                        });
                        dataContext.SaveChanges();
                        statusCode = StatusCodes.Status201Created;
                        result = $"Дані внесено: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                    }
                }
                catch
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    result = $"Дані не опрацьовані: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                }
            }


            HttpContext.Response.StatusCode = statusCode;
            return new { result, statusCode };
        }
        public object Default()
        {
            switch (HttpContext.Request.Method)
            {
                case "LINK": return Link();
                default: throw new NotImplementedException();
            }

        }
        [HttpDelete]
        public Object Delete([FromBody] BodyData bodyData)
        {

            int statusCode;
            String result;

            if (bodyData == null
                || bodyData.Data == null
                || bodyData.ItemId == null
                || bodyData.UserId == null)
            {
                statusCode = StatusCodes.Status400BadRequest;
                result = $"Не всі дані передано: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
            }
            else
            {
                try
                {
                    Guid itemId = Guid.Parse(bodyData.ItemId);
                    Guid userId = Guid.Parse(bodyData.UserId);
                    int rating = Convert.ToInt32(bodyData.Data);

                    Rate? rate = dataContext.Rates.FirstOrDefault(r => r.UserId == userId && r.ItemId == itemId);
                    if (dataContext.Rates.Any(r => r.UserId == userId && r.ItemId == itemId))
                    {
                        dataContext.Rates.Remove(rate);
                        dataContext.SaveChanges();
                        statusCode = StatusCodes.Status202Accepted;
                        result = $"Дані видалено: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";

                    }
                    else
                    {
                        statusCode = StatusCodes.Status406NotAcceptable;
                        result = $"Дані відсутні (не можуть бути видалені): ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                    }
                }
                catch
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    result = $"Дані не опрацьовані: Data={bodyData?.Data} ItemId={bodyData?.ItemId} UserId={bodyData?.UserId}";
                }
            }


            HttpContext.Response.StatusCode = statusCode;
            return new { result, statusCode };
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
        public String? Data { get; set; } = null!;
        public String? ItemId { get; set; } = null!;
        public String? UserId { get; set; } = null!;
    }
}
