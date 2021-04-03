using System;
using System.Collections.Generic;

namespace BlazorSurveys.Shared
{
    public record Survey : IExpirable
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Title { get; init; }
        public DateTime ExpiresAt { get; init; }
        public List<string> Options { get; init; } = new List<string>();
        public List<SurveyAnswer> Answers { get; init; } = new List<SurveyAnswer>();

        public SurveySummary ToSummary() => new SurveySummary
        {
            Id = Id,
            Title = Title,
            Options = Options,
            ExpiresAt = ExpiresAt
        };
    }

    public record SurveyAnswer
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid SurveyId { get; init; }
        public string Option { get; init; }
    }

    public record SurveySummary : IExpirable
    {
        public Guid Id { get; init; }
        public string Title { get; init; }
        public DateTime ExpiresAt { get; init; }
        public List<string> Options { get; init; }
    }
}
