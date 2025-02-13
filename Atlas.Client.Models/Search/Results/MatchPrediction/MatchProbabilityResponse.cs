﻿using System;
using System.Collections.Generic;
using Atlas.Common.Public.Models.GeneticData;
using Atlas.Common.Public.Models.GeneticData.PhenotypeInfo;
using Atlas.Common.Public.Models.GeneticData.PhenotypeInfo.TransferModels;
using Atlas.Common.Public.Models.MatchPrediction;
using Newtonsoft.Json;

namespace Atlas.Client.Models.Search.Results.MatchPrediction
{
    public class MatchProbabilityResponse
    {
        public MatchProbabilities MatchProbabilities { get; set; }

        [JsonIgnore]
        public LociInfo<MatchProbabilityPerLocusResponse> MatchProbabilitiesPerLocus { get; set; }

        [JsonProperty(nameof(MatchProbabilitiesPerLocus))]
        public LociInfoTransfer<MatchProbabilityPerLocusResponse> MatchProbabilitiesPerLocusTransfer
        {
            get => MatchProbabilitiesPerLocus.ToLociInfoTransfer();
            set => MatchProbabilitiesPerLocus = value.ToLociInfo();
        }

        public int? OverallMatchCount => ExactMatchCount + PotentialMatchCount;
        public int? ExactMatchCount => GetMatchCount(PredictiveMatchCategory.Exact);
        public int? PotentialMatchCount => GetMatchCount(PredictiveMatchCategory.Potential);

        public PredictiveMatchCategory? OverallMatchCategory => MatchProbabilities.MatchCategory;

        [JsonIgnore]
        public LociInfo<Probability> ZeroMismatchProbabilityPerLocus =>
            MatchProbabilitiesPerLocus.Map(x => x?.MatchProbabilities.ZeroMismatchProbability);

        [JsonIgnore]
        public LociInfo<Probability> OneMismatchProbabilityPerLocus =>
            MatchProbabilitiesPerLocus.Map(x => x?.MatchProbabilities.OneMismatchProbability);

        [JsonIgnore]
        public LociInfo<Probability> TwoMismatchProbabilityPerLocus =>
            MatchProbabilitiesPerLocus.Map(x => x?.MatchProbabilities.TwoMismatchProbability);

        /// <summary>
        /// Nomenclature version used for HF set generation + processing for the patient.
        /// NOTE: HLA expansion of the request was done using the input version.
        /// This property is set as readonly and is only used for backwards compatibility.
        /// </summary>
        [Obsolete("The property is moved to the PatientHaplotypeFrequencySet")]
        public string PatientFrequencySetNomenclatureVersion => PatientHaplotypeFrequencySet?.HlaNomenclatureVersion;

        public HaplotypeFrequencySet PatientHaplotypeFrequencySet { get; set; }

        /// <summary>
        /// Nomenclature version used for HF set generation + processing for the donor.
        /// NOTE: HLA expansion of the request was done using the input version.
        /// This property is set as readonly and is only used for backwards compatibility.
        /// </summary>
        [Obsolete("The property is moved to the DonorHaplotypeFrequencySet")]
        public string DonorFrequencySetNomenclatureVersion => DonorHaplotypeFrequencySet?.HlaNomenclatureVersion;

        public HaplotypeFrequencySet DonorHaplotypeFrequencySet { get; set;}

        public bool IsPatientPhenotypeUnrepresented { get; set; }
        public bool IsDonorPhenotypeUnrepresented { get; set; }

        public MatchProbabilityResponse()
        {
        }

        /// <summary>
        /// Used to initialise a response when all probabilities are known upfront.
        /// This can be useful for e.g. Shortcuts when a mismatch is guaranteed.
        /// </summary>
        public MatchProbabilityResponse(Probability sharedProbability, ISet<Locus> allowedLoci)
        {
            MatchProbabilities = new MatchProbabilities(sharedProbability);
            MatchProbabilitiesPerLocus = new LociInfo<Probability>(sharedProbability).Map((l, v) =>
                allowedLoci.Contains(l) ? new MatchProbabilityPerLocusResponse(sharedProbability) : null
            );
        }

        public MatchProbabilityResponse Round(int decimalPlaces)
        {
            return new MatchProbabilityResponse
            {
                MatchProbabilities = MatchProbabilities.Round(decimalPlaces),
                MatchProbabilitiesPerLocus = MatchProbabilitiesPerLocus.Map((_, p) => p?.Round(decimalPlaces)),
                IsPatientPhenotypeUnrepresented = IsPatientPhenotypeUnrepresented,
                IsDonorPhenotypeUnrepresented = IsDonorPhenotypeUnrepresented,
                PatientHaplotypeFrequencySet = PatientHaplotypeFrequencySet,
                DonorHaplotypeFrequencySet = DonorHaplotypeFrequencySet
            };
        }

        private int? GetMatchCount(PredictiveMatchCategory matchCategory)
        {
            if (IsDonorPhenotypeUnrepresented || IsPatientPhenotypeUnrepresented)
            {
                return null;
            }

            return MatchProbabilitiesPerLocus.Reduce((locus, value, accumulator) =>
            {
                if (value?.PositionalMatchCategories?.Position1 == matchCategory)
                {
                    accumulator++;
                }

                if (value?.PositionalMatchCategories?.Position2 == matchCategory)
                {
                    accumulator++;
                }

                return accumulator;
            }, 0);
        }
    }
}