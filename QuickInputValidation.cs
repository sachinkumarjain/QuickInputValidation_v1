using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickInputValidation
{
    public abstract class BaseValidationFor<TValidatableObject> : QuickValidationFor<TValidatableObject> where TValidatableObject : class, new()
    {
        public IQuickValidationFor<TValidatableObject> ValidationFor() { return this; }
    }

    //Core logic to validate the validatable object
    public interface IQuickValidationFor<TValidatableObject>
    {
        IQuickValidationFor<TValidatableObject> ShouldBe(Func<TValidatableObject, bool> shouldBeTrue,
            Func<TValidatableObject, string> message);
        IQuickValidationFor<TValidatableObject> ShouldBe(Func<TValidatableObject, bool> shouldBeTrue, string message);
        IQuickValidationFor<TValidatableObject> ShouldBe(Func<TValidatableObject, bool> shouldBeTrue);
        IQuickValidationFor<TValidatableObject> With(Func<TValidatableObject, bool> shouldBeTrue);
        FailureResult Validate(TValidatableObject request);
    }
    public class QuickValidationFor<TValidatableObject> : IQuickValidationFor<TValidatableObject>
    {
        private const string PRE_CONDITION = "_PRE_CONDITION_";
        protected IDictionary<Func<TValidatableObject, bool>, Func<TValidatableObject, string>> RulesFor { get; set; }

        protected IList<string> failures { get; set; }
        protected QuickValidationFor()
        {
            RulesFor = new Dictionary<Func<TValidatableObject, bool>, Func<TValidatableObject, string>>();
            failures = new List<string>();
        }

        private IQuickValidationFor<TValidatableObject> SetValidator(Func<TValidatableObject, bool> shouldBeTrue,
            Func<TValidatableObject, string> message)
        {
            RulesFor.Add(shouldBeTrue, message);
            return this;
        }

        public bool StopToContinue { get; set; }

        public string[] ErrorMessages { get { return failures.ToArray(); } }
        public FailureResult Validate(TValidatableObject request)
        {
            bool? isPreCondition = null;
            foreach (var rule in RulesFor)
            {
                if (rule.Value(request) == PRE_CONDITION)
                {
                    isPreCondition = rule.Key(request);
                    continue;
                }
                var isValid = isPreCondition != null ? isPreCondition.Value && rule.Key(request) : rule.Key(request);

                if (!isValid)
                {
                    failures.Add(rule.Value(request));
                    if (StopToContinue) { break; }
                }
            }
            return new FailureResult(failures);
        }

        public IQuickValidationFor<TValidatableObject> ShouldBe(Func<TValidatableObject, bool> shouldBeTrue,
            Func<TValidatableObject, string> message)
        {
            return SetValidator(shouldBeTrue, message);
        }
        public IQuickValidationFor<TValidatableObject> ShouldBe(Func<TValidatableObject, bool> shouldBeTrue, string message)
        {
            return SetValidator(shouldBeTrue, (TValidatableObject t) => { return message; });
        }
        public IQuickValidationFor<TValidatableObject> ShouldBe(Func<TValidatableObject, bool> shouldBeTrue)
        {
            return SetValidator(shouldBeTrue, (TValidatableObject t) => { return string.Format("{0} is not valid", t.GetType().Name); });
        }

        private Func<TValidatableObject, bool> preCondition;
        //
        public IQuickValidationFor<TValidatableObject> With(Func<TValidatableObject, bool> shouldBeTrue)
        {
            preCondition = shouldBeTrue;
            return SetValidator(shouldBeTrue, (TValidatableObject t) => { return PRE_CONDITION; });
        }
    }
    public class FailureResult
    {
        public FailureResult(IList<string> failures)
        {
            IsValid = !failures.Any();
            ErrorMessages = failures.ToArray();
        }

        public bool IsValid { get; private set; }
        public string[] ErrorMessages { get; private set; }
        public string ErrorMessage { get { return string.Join(", ", ErrorMessages); } }
    }

    //Helper class to exptend for re-usable methods
    public static class ValidationHelper
    {
        public static bool IsNumber(this string s)
        {
            var num = 0;
            return int.TryParse(s, out num);
        }

        public static bool NotEmpty(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

        public static bool NotNull(this string s)
        {
            return s != null;
        }

        public static bool Length(this string s, int size)
        {
            return s.NotEmpty() && s.Length == size;
        }

        public static bool Length(this string s, int min, int max)
        {
            return s.NotEmpty() && s.Length >= min && s.Length <= max;
        }
    }
}
