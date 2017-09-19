using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterPatientIndex.ProbabilisticMPI
{
    public class WeightsCalculator
    {

        public IList<MPIIdentifierWeight> SetIdentifierWeights(IList<MatchAggregateResultItem> matchAggregateResult)
        {
            var result = matchAggregateResult
                .Select(CalculateWeights)
                .ToList();

            return result;
        }

        private MPIIdentifierWeight CalculateWeights(MatchAggregateResultItem matchAggregateResultItem)
        {
            // http://en.wikipedia.org/wiki/Record_linkage
            // Formula used to calculate match/non-match weights for identifiers:  (example: birth month)
            // The m probability is the probability that an identifier in matching pairs will agree (or be sufficiently similar, such as strings with high Jaro-Winkler distance or low Levenshtein distance)
            // The u probability is the probability that an identifier in two non-matching records will agree purely by chance.
            //  the u probability for birth month (where there are twelve values that are approximately uniformly distributed) is 1/12 ≈ 0.083; 
            //      identifiers with values that are not uniformly distributed will have different u probabilities for different values (possibly including missing values)
            // Outcome   Proportion of links  Proportion of non-links    Frequency ratio         Weight
            // Match     m = 0.95             u ≈ 0.083                  m/u ≈ 11.4              ln(m/u)/ln(2) ≈ 3.51 
            // Non-match 1−m = 0.05          1-u ≈ 0.917                 (1-m)/(1-u) ≈ 0.0545    ln((1-m)/(1-u))/ln(2) ≈ -4.20 

            var m = matchAggregateResultItem.ProportionOfCorrectMatches;
            var u = matchAggregateResultItem.ProportionOfIncorrectMatches; // (double)0.083; //TODO: check this
            var matchFrequencyRatio = m / u;
            var matchWeight = Math.Log(matchFrequencyRatio) / Math.Log(2);

            var nonMatchFrequencyRatio = (1 - m) / (1 - u);
            var nonMatchWeight = Math.Log(nonMatchFrequencyRatio) / Math.Log(2);

            return new MPIIdentifierWeight
            {
                Identifier = matchAggregateResultItem.IdentifierName,
                MatchWeight = matchWeight,
                NonMatchWeight = nonMatchWeight,
            };

        }
    }

    public class MatchAggregateResultItem
    {
        public string IdentifierName { get; set; }
        public double ProportionOfCorrectMatches { get; set; }
        public double ProportionOfIncorrectMatches { get; set; }
    }
}
