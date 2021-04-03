using BlazorSurveys.Server.Hubs;
using BlazorSurveys.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorSurveys.Server.Controllers
{   
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {
        private readonly IHubContext<SurveyHub, ISurveyHub> hubContext;
        private static ConcurrentBag<Survey> surveys = new ConcurrentBag<Survey> {
            // feel free to initialize here some sample surveys like:
            new Survey {
                Id = Guid.Parse("b00c58c0-df00-49ac-ae85-0a135f75e01b"),
                Title = "Are you excited about .NET 5.0?",
                ExpiresAt = DateTime.Now.AddMinutes(10),
                Options = new List<string>{ "Yes", "Nope", "meh" },
                Answers = new List<SurveyAnswer>{
                    new SurveyAnswer { Option = "Yes" },
                    new SurveyAnswer { Option = "Yes" },
                    new SurveyAnswer { Option = "Yes" },
                    new SurveyAnswer { Option = "Nope" },
                    new SurveyAnswer { Option = "meh" }
                }
            }
        };

        public SurveyController(IHubContext<SurveyHub, ISurveyHub> surveyHub)
        {
            hubContext = surveyHub;
        }

        [HttpGet()]
        public IEnumerable<SurveySummary> GetSurveys()
        {
            return surveys.Select(s => s.ToSummary());
        }

        [HttpGet("{id}")]
        public ActionResult GetSurvey(Guid id)
        {
            var survey = surveys.SingleOrDefault(t => t.Id == id);
            if (survey == null) return NotFound();
            return new JsonResult(survey);
        }

        [HttpPut()]
        public async Task<Survey> AddSurvey([FromBody] AddSurveyModel addSurveyModel)
        {
            var survey = new Survey
            {
                Title = addSurveyModel.Title,
                ExpiresAt = DateTime.Now.AddMinutes(addSurveyModel.Minutes.Value),
                Options = addSurveyModel.Options.Select(o => o.OptionValue).ToList()
            };
            surveys.Add(survey);
            await hubContext.Clients.All.SurveyAdded(survey.ToSummary());
            return survey;
        }

        [HttpPost("{surveyId}/answer")]
        public async Task<ActionResult> AnswerSurvey(Guid surveyId, [FromBody]SurveyAnswer answer)
        {
            var survey = surveys.SingleOrDefault(t => t.Id == surveyId);
            if (survey == null) return NotFound();

            if (((IExpirable)survey).IsExpired) return StatusCode(400, "This survey hasexpired");

            // WARNING: this isn’t thread safe since we store answers in a List!
            survey.Answers.Add(new SurveyAnswer
            {
                SurveyId = surveyId,
                Option = answer.Option
            });

            await hubContext.Clients.Group(surveyId.ToString()).SurveyUpdated(survey);
            return new JsonResult(survey);
        }
    }
}
