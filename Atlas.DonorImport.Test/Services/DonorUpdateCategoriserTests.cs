﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlas.Common.ApplicationInsights;
using Atlas.DonorImport.ApplicationInsights;
using Atlas.DonorImport.Data.Models;
using Atlas.DonorImport.Data.Repositories;
using Atlas.DonorImport.FileSchema.Models;
using Atlas.DonorImport.Services;
using Atlas.DonorImport.Test.TestHelpers.Builders;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Atlas.DonorImport.Test.Services
{
    [TestFixture]
    internal class DonorUpdateCategoriserTests
    {
        private IDonorUpdateCategoriser categoriser;
        private ILogger logger;
        private IDonorReadRepository donorReadRepository;
        private IDonorImportFailureRepository donorImportFailureRepository;


        [SetUp]
        public void SetUp()
        {
            logger = Substitute.For<ILogger>();
            donorReadRepository = Substitute.For<IDonorReadRepository>();
            donorImportFailureRepository = Substitute.For<IDonorImportFailureRepository>();
            categoriser = new DonorUpdateCategoriser(logger, donorReadRepository, donorImportFailureRepository);
        }

        [Test]
        public async Task Categorise_CategorisesDonorUpdatesAsValidOrInvalid()
        {
            var validDonorUpdate = DonorUpdateBuilder.New.Build(1).ToList();
            var invalidDonorUpdate = DonorUpdateBuilder.New.WithHla(null).Build(1).ToList();

            var result = await categoriser.Categorise(validDonorUpdate.Concat(invalidDonorUpdate), string.Empty);

            result.ValidDonors.Select(d => d.RecordId).Should().BeEquivalentTo(validDonorUpdate.Select(d => d.RecordId));
            result.InvalidDonors.Select(d => d.RecordId).Should().BeEquivalentTo(invalidDonorUpdate.Select(d => d.RecordId));
        }

        [Test]
        public async Task Categorise_LogsOneEventForEachUniqueValidationError()
        {
            const int noHlaCount = 3;
            var invalidNoHla = DonorUpdateBuilder.New.WithHla(null).Build(noHlaCount).ToList();

            const int noDrb1Count = 5;
            var hlaMissingDrb1 = HlaBuilder.Default.WithValidHlaAtAllLoci().With(x => x.DRB1, (ImportedLocus)null).Build();
            var invalidMissingDrb1 = DonorUpdateBuilder.New.WithHla(hlaMissingDrb1).Build(noDrb1Count);

            var result = await categoriser.Categorise(invalidNoHla.Concat(invalidMissingDrb1), string.Empty);

            result.ValidDonors.Should().BeEmpty();
            result.InvalidDonors.Should().HaveCount(noHlaCount + noDrb1Count);
            logger.Received(2).SendEvent(Arg.Any<SearchableDonorValidationErrorEventModel>());
        }

        [Test]
        public async Task Categorise_ReadsExternalDonorCodesToCreateValidatorContext()
        {
            var donorUpdate = DonorUpdateBuilder.New.Build();

            await categoriser.Categorise(new [] { donorUpdate }, string.Empty);

            await donorReadRepository.Received().GetExistingExternalDonorCodes(Arg.Is<IEnumerable<string>>(l => l.Contains(donorUpdate.RecordId)));
        }


        [Test]
        public async Task Categorise_SavesDonorImportFailureForInvalidDonor()
        {
            const string fileName = "fileName";
            var validDonorUpdate = DonorUpdateBuilder.New.Build(1).ToList();
            var invalidDonorUpdate = DonorUpdateBuilder.New.WithHla(null).Build();

            await categoriser.Categorise(validDonorUpdate.Concat(new[] { invalidDonorUpdate }), fileName);
            
            await donorImportFailureRepository.BulkInsert(
                Arg.Is<IReadOnlyCollection<DonorImportFailure>>(l => l.Any(f => f.ExternalDonorCode == invalidDonorUpdate.RecordId && f.UpdateFile == fileName)));
        }
    }
}
