﻿using Atlas.Common.Public.Models.GeneticData.PhenotypeInfo;
using Atlas.Common.Public.Models.GeneticData.PhenotypeInfo.TransferModels;
using Atlas.Common.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Atlas.Common.Test.Validation
{
    [TestFixture]
    public class RequiredLocusHlaNamesValidatorTests
    {
        private RequiredLocusHlaNamesValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new RequiredLocusHlaNamesValidator();
        }

        [Test]
        public void Validator_WhenNoHlaStringsAreProvided_ShouldHaveValidationError()
        {
            var locusHlaNames = new LocusInfoTransfer<string>();
            var result = validator.Validate(locusHlaNames);
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void Validator_WhenEmptyHlaStringsAreProvided_ShouldHaveValidationError()
        {
            var locusHlaNames = new LocusInfo<string>("").ToLocusInfoTransfer();
            var result = validator.Validate(locusHlaNames);
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void Validator_WhenOnlyFirstHlaStringProvided_ShouldHaveValidationError()
        {
            var locusHlaNames = new LocusInfo<string>("hla", null).ToLocusInfoTransfer();
            var result = validator.Validate(locusHlaNames);
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void Validator_WhenOnlySecondHlaStringProvided_ShouldHaveValidationError()
        {
            var locusHlaNames = new LocusInfo<string>(null, "hla").ToLocusInfoTransfer();
            var result = validator.Validate(locusHlaNames);
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void Validator_WhenBothHlaStringsProvided_ShouldNotHaveValidationError()
        {
            var locusHlaNames = new LocusInfo<string>("hla-string-1", "hla-string-2").ToLocusInfoTransfer();
            var result = validator.Validate(locusHlaNames);
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validator_WhenFirstHlaStringNull_ShouldHaveValidationError()
        {
            var locusHlaNames = new LocusInfo<string>(null, "not-null").ToLocusInfoTransfer();
            var result = validator.Validate(locusHlaNames);
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void Validator_WhenSecondHlaStringNull_ShouldHaveValidationError()
        {
            var locusHlaNames = new LocusInfo<string>("not-null", null).ToLocusInfoTransfer();
            var result = validator.Validate(locusHlaNames);
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void Validator_WhenBothHlaStringsNull_ShouldHaveValidationError()
        {
            var locusHlaNames = new LocusInfo<string>(null as string).ToLocusInfoTransfer();
            var result = validator.Validate(locusHlaNames);
            result.IsValid.Should().BeFalse();
        }
    }
}