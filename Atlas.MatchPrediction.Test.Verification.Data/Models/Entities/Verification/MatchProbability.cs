﻿using System.ComponentModel.DataAnnotations.Schema;
using Atlas.Common.GeneticData;
using Atlas.Common.Public.Models.GeneticData;
using Atlas.Common.Sql.BulkInsert;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.MatchPrediction.Test.Verification.Data.Models.Entities.Verification
{
    // ReSharper disable InconsistentNaming

    public class MatchProbability : IBulkInsertModel
    {
        public int Id { get; set; }
        public int MatchedDonor_Id { get; set; }

        /// <summary>
        /// This will be null for "cross-loci" predictions, e.g., P(x/10), etc.
        /// </summary>
        [Column(TypeName = "nvarchar(10)")]
        public Locus? Locus { get; set; }

        public int MismatchCount { get; set; }

        /// <summary>
        /// Nullable for when patient and/or donor were non-represented.
        /// </summary>
        [Column(TypeName = "decimal(6,5)")]
        public decimal? Probability { get; set; }
    }

    internal static class MatchProbabilityBuilder
    {
        public static void SetUpModel(this EntityTypeBuilder<MatchProbability> modelBuilder)
        {
            modelBuilder
                .HasOne<MatchedDonor>()
                .WithMany()
                .HasForeignKey(r => r.MatchedDonor_Id);
        }
    }
}
