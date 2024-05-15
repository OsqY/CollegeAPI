using CollegeAPI.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CollegeAPI.DTO
{
    public class RequestDTO<T> : IValidatableObject
    {
        [DefaultValue(0)]
        public int PageIndex { get; set; } = 0;

        [DefaultValue(10)]
        public int PageSize { get; set; } = 10;

        [DefaultValue("Name")]
        public string? SortColumn { get; set; } = "Name";

        [SortOrderValidator]
        [DefaultValue("ASC")]
        public string? SortOrder { get; set; } = "ASC";

        [DefaultValue(null)]
        public string? FilterQuery { get; set; } = null;

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            var validator = new SortColumnValidatorAttribute(typeof(T));
            var result = validator.GetValidationResult(SortColumn, context);

            return (result != null) ? new[] { result } : new ValidationResult[0];

        }

    }
}
