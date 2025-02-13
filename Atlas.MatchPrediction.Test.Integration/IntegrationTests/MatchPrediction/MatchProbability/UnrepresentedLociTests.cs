﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Common.GeneticData;
using Atlas.Common.GeneticData.PhenotypeInfo;
using Atlas.Common.Public.Models.GeneticData;
using Atlas.Common.Public.Models.GeneticData.PhenotypeInfo;
using Atlas.Common.Public.Models.MatchPrediction;
using Atlas.MatchPrediction.Data.Models;
using Atlas.MatchPrediction.Test.TestHelpers.Builders.MatchProbabilityInputs;
using FluentAssertions;
using NUnit.Framework;

namespace Atlas.MatchPrediction.Test.Integration.IntegrationTests.MatchPrediction.MatchProbability
{
    public class UnrepresentedLociTests : MatchProbabilityTestsBase
    {
        // the default "ambiguous alleles builder" is not actually ambiguous with respect to g-groups
        // - to show up as unrepresented, there must be some ambiguity at the g-group level in the input genotype
        private PhenotypeInfo<string> ambiguousGenotype = DefaultAmbiguousAllelesBuilder
            .WithDataAt(Locus.A, LocusPosition.One, "01:XX")
            .Build();

        [Test]
        public async Task CalculateMatchProbability_WhenUnrepresentedPatientAndDonor_ReturnsNullProbabilityAndPatientAndDonorUnrepresentedFlagsTrue()
        {
            var matchProbabilityInput = DefaultInputBuilder
                .WithDonorHla(ambiguousGenotype)
                .WithPatientHla(ambiguousGenotype)
                .Build();

            var possibleHaplotypes = new List<HaplotypeFrequency>
            {
                DefaultHaplotypeFrequency1.With(h => h.Frequency, 0.00002m).Build(),
                DefaultHaplotypeFrequency2.With(h => h.Frequency, 0.00001m).Build(),
            };

            await ImportFrequencies(possibleHaplotypes, null, null);

            var expectedMismatchProbabilityPerLocus = new LociInfo<Probability>((Probability)null);

            var matchDetails = await MatchProbabilityService.CalculateMatchProbability(matchProbabilityInput);
            var roundedMatchDetails = matchDetails.Round(4);

            roundedMatchDetails.MatchProbabilities.ZeroMismatchProbability.Should().Be(null);
            roundedMatchDetails.MatchProbabilities.OneMismatchProbability.Should().Be(null);
            roundedMatchDetails.MatchProbabilities.TwoMismatchProbability.Should().Be(null);
            roundedMatchDetails.ZeroMismatchProbabilityPerLocus.Should().Be(expectedMismatchProbabilityPerLocus);
            roundedMatchDetails.OneMismatchProbabilityPerLocus.Should().Be(expectedMismatchProbabilityPerLocus);
            roundedMatchDetails.TwoMismatchProbabilityPerLocus.Should().Be(expectedMismatchProbabilityPerLocus);
            roundedMatchDetails.IsDonorPhenotypeUnrepresented.Should().Be(true);
            roundedMatchDetails.IsPatientPhenotypeUnrepresented.Should().Be(true);
        }

        [Test]
        public async Task CalculateMatchProbability_WhenUnrepresentedPatient_ReturnsNullProbabilityAndPatientUnrepresentedFlagTrue()
        {
            var matchProbabilityInput = DefaultInputBuilder.WithPatientHla(ambiguousGenotype).Build();

            var possibleHaplotypes = new List<HaplotypeFrequency>
            {
                DefaultHaplotypeFrequency1.With(h => h.Frequency, 0.00002m).Build(),
                DefaultHaplotypeFrequency2.With(h => h.Frequency, 0.00001m).Build(),
            };

            await ImportFrequencies(possibleHaplotypes, null, null);

            var expectedMismatchProbabilityPerLocus = new LociInfo<Probability>((Probability)null);

            var matchDetails = await MatchProbabilityService.CalculateMatchProbability(matchProbabilityInput);
            var roundedMatchDetails = matchDetails.Round(4);

            roundedMatchDetails.MatchProbabilities.ZeroMismatchProbability.Should().Be(null);
            roundedMatchDetails.MatchProbabilities.OneMismatchProbability.Should().Be(null);
            roundedMatchDetails.MatchProbabilities.TwoMismatchProbability.Should().Be(null);
            roundedMatchDetails.ZeroMismatchProbabilityPerLocus.Should().Be(expectedMismatchProbabilityPerLocus);
            roundedMatchDetails.OneMismatchProbabilityPerLocus.Should().Be(expectedMismatchProbabilityPerLocus);
            roundedMatchDetails.TwoMismatchProbabilityPerLocus.Should().Be(expectedMismatchProbabilityPerLocus);
            roundedMatchDetails.IsDonorPhenotypeUnrepresented.Should().Be(false);
            roundedMatchDetails.IsPatientPhenotypeUnrepresented.Should().Be(true);
        }

        [Test]
        public async Task CalculateMatchProbability_WhenUnrepresentedDonor_ReturnsNullProbabilityAndDonorUnrepresentedFlagTrue()
        {
            var matchProbabilityInput = DefaultInputBuilder.WithDonorHla(ambiguousGenotype).Build();

            var possibleHaplotypes = new List<HaplotypeFrequency>
            {
                DefaultHaplotypeFrequency1.With(h => h.Frequency, 0.00002m).Build(),
                DefaultHaplotypeFrequency2.With(h => h.Frequency, 0.00001m).Build(),
            };

            await ImportFrequencies(possibleHaplotypes, null, null);

            var expectedMismatchProbabilityPerLocus = new LociInfo<Probability>((Probability)null);

            var matchDetails = await MatchProbabilityService.CalculateMatchProbability(matchProbabilityInput);
            var roundedMatchDetails = matchDetails.Round(4);

            roundedMatchDetails.MatchProbabilities.ZeroMismatchProbability.Should().Be(null);
            roundedMatchDetails.MatchProbabilities.OneMismatchProbability.Should().Be(null);
            roundedMatchDetails.MatchProbabilities.TwoMismatchProbability.Should().Be(null);
            roundedMatchDetails.ZeroMismatchProbabilityPerLocus.Should().Be(expectedMismatchProbabilityPerLocus);
            roundedMatchDetails.OneMismatchProbabilityPerLocus.Should().Be(expectedMismatchProbabilityPerLocus);
            roundedMatchDetails.TwoMismatchProbabilityPerLocus.Should().Be(expectedMismatchProbabilityPerLocus);
            roundedMatchDetails.IsDonorPhenotypeUnrepresented.Should().Be(true);
            roundedMatchDetails.IsPatientPhenotypeUnrepresented.Should().Be(false);
        }
    }
}