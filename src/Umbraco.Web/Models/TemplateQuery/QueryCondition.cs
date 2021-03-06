﻿namespace Umbraco.Web.Models.TemplateQuery
{
    public class QueryCondition
    {

        public PropertyModel Property { get; set; }
        public OperatorTerm Term { get; set; }
        public string ConstraintValue { get; set; }
    }


    internal static class QueryConditionExtensions
    {

        public static string BuildTokenizedCondition(this QueryCondition condition, int token)
        {
            return condition.BuildConditionString(string.Empty, token);
        }

        public static string BuildCondition(this QueryCondition condition, string parameterAlias)
        {
            return condition.BuildConditionString(parameterAlias + ".");
        }

        private static string BuildConditionString(this QueryCondition condition, string prefix, int token = -1)
        {



            var operand = string.Empty;
            var value = string.Empty;
            var constraintValue = string.Empty;


            //if a token is used, use a token placeholder, otherwise, use the actual value
            if(token >= 0){
                constraintValue = string.Format("@{0}", token);
            }else {

                //modify the format of the constraint value
                switch (condition.Property.Type)
                {
                    case "string":
                        constraintValue = string.Format("\"{0}\"", condition.ConstraintValue);
                        break;
                    case "datetime":
                        constraintValue = string.Format("DateTime.Parse(\"{0}\")", condition.ConstraintValue);
                        break;
                    default:
                        constraintValue = condition.ConstraintValue;
                        break;
                }

            }

            switch (condition.Term.Operator)
            {
                case Operator.Equals:
                    operand =   " == ";
                    break;
                case Operator.NotEquals:
                    operand = " != ";
                    break;
                case Operator.GreaterThan:
                    operand = " > ";
                    break;
                case Operator.GreaterThanEqualTo:
                    operand = " >= ";
                    break;
                case Operator.LessThan:
                    operand = " < ";
                    break;
                case Operator.LessThanEqualTo:
                    operand = " <= ";
                    break;
                case Operator.Contains:
                    value = string.Format("{0}{1}.Contains({2})", prefix, condition.Property.Alias, constraintValue);
                    break;
                case Operator.NotContains:
                    value =  string.Format("!{0}{1}.Contains({2})", prefix, condition.Property.Alias, constraintValue);
                    break;
                default :
                    operand = " == ";
                    break;
            }


            if (string.IsNullOrEmpty(value) == false)
                return value;



            return string.Format("{0}{1}{2}{3}", prefix, condition.Property.Alias, operand, constraintValue);
        }

    }
}
