﻿using System.Collections.Generic;
using System.Linq;
using System.IO;
using Pisces.IO;
using Pisces.Domain.Models;
using Pisces.IO.Sequencing;
using Pisces.Domain.Options;
using VariantPhasing.Logic;
using VariantPhasing.Models;
using Xunit;

namespace VariantPhasing.Tests.Logic
{
    public class VcfNeighborhoodBuilderTests
    {
        string _sourceVcf0 = Path.Combine(TestPaths.LocalTestDataDirectory, "NbhdBuilderTest0.genome.vcf");
        string _sourceVcf1 = Path.Combine(TestPaths.LocalTestDataDirectory, "NbhdBuilderTest1.genome.vcf");
        string _sourceVcf2 = Path.Combine(TestPaths.LocalTestDataDirectory, "NbhdBuilderTest2.genome.vcf");
        string _sourceVcf3 = Path.Combine(TestPaths.LocalTestDataDirectory, "NbhdBuilderTest3.genome.vcf");
        string _sourceVcf4 = Path.Combine(TestPaths.LocalTestDataDirectory, "NbhdBuilderTest4.genome.vcf");
        string _sourceVcf5 = Path.Combine(TestPaths.LocalTestDataDirectory, "NbhdBuilderTest5.genome.vcf");
        string _sourceVcf_Mutated = Path.Combine(TestPaths.LocalTestDataDirectory, "VeryMutated.genome.vcf");

        [Fact]
        public void GetNeighborhoodsFromMessyVCF()
        {
            var vcfFilePath = Path.Combine(TestPaths.LocalTestDataDirectory, "VeryMutated.genome.vcf");

            var neighborhoodManager = CreateNbhdBuilder(_sourceVcf0);
            var numRaw = 0;
            Assert.Equal(0, neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw).Count());
            Assert.Equal(0, numRaw);

            neighborhoodManager = CreateNbhdBuilder(_sourceVcf_Mutated);

            var neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(1, neighborhoods.Count());
            Assert.Equal(1, numRaw);
            Assert.Equal(12, neighborhoods.First().VcfVariantSites.Count());

        }

        [Fact]
        public void CreateCallableNbhdsTests()
        {
            var vcfFilePath = Path.Combine(TestPaths.LocalTestDataDirectory, "VeryMutated.genome.vcf");
            var variantSource = new AlleleReader(vcfFilePath);
            var vcfNeighborhood = new VcfNeighborhood(0, "chr1", new VariantSite(123), new VariantSite(125));
            List<VcfNeighborhood> VcfNeighborhoods = new List<VcfNeighborhood>() { vcfNeighborhood };

            //Test 1, genome is NULL

            var neighborhoodBuilder = new NeighborhoodBuilder(new PhasableVariantCriteria(), new VariantCallingParameters(),
                variantSource, null, 20);

            var neighborhoods = neighborhoodBuilder.ConvertToCallableNeighborhoods(VcfNeighborhoods);
            Assert.Equal(1, neighborhoods.Count());
            Assert.Equal(2, neighborhoods.First().VcfVariantSites.Count());
            Assert.Equal("chr1", neighborhoods[0].ReferenceName);
            Assert.Equal("RRR", neighborhoods[0].NbhdReferenceSequenceSubstring);

            //Test 2, genome is exists, but doesnt have the right chr

            var genomePath = Path.Combine(TestPaths.SharedGenomesDirectory, "Bacillus_cereus", "Sequence", "WholeGenomeFasta");
            var refName = "chr_wrong";
            Genome genome = new Genome(genomePath, new List<string>() { refName });
            ChrReference chrReference = genome.GetChrReference(refName);

            neighborhoodBuilder = new NeighborhoodBuilder(new PhasableVariantCriteria(), new VariantCallingParameters(),
                variantSource, genome, 20);

            neighborhoods = neighborhoodBuilder.ConvertToCallableNeighborhoods(VcfNeighborhoods);
            Assert.Equal(1, neighborhoods.Count());
            Assert.Equal(2, neighborhoods.First().VcfVariantSites.Count());
            Assert.Equal("chr1", neighborhoods[0].ReferenceName);
            Assert.Equal("RRR", neighborhoods[0].NbhdReferenceSequenceSubstring);


            //Test 3, genome is exists, and DOES have the right chr

            refName = "chr";
            genome = new Genome(genomePath, new List<string>() { refName });
            chrReference = genome.GetChrReference(refName);

            neighborhoodBuilder = new NeighborhoodBuilder(new PhasableVariantCriteria(), new VariantCallingParameters(),
                variantSource, genome, 20);


            vcfNeighborhood = new VcfNeighborhood(0, "chr", new VariantSite(123), new VariantSite(125));
            VcfNeighborhoods = new List<VcfNeighborhood>() { vcfNeighborhood };

            neighborhoods = neighborhoodBuilder.ConvertToCallableNeighborhoods(VcfNeighborhoods);
            Assert.Equal(1, neighborhoods.Count());
            Assert.Equal(2, neighborhoods.First().VcfVariantSites.Count());
            Assert.Equal("chr", neighborhoods[0].ReferenceName);
            Assert.Equal("TAT", neighborhoods[0].NbhdReferenceSequenceSubstring);
        }

        [Fact]
        public void GetNeighborhoods()
        {
            var variant = CreateVariant(123);
            var secondVariant = CreateVariant(124);
            var thirdVariant = CreateVariant(125);
            var fourthVariant = CreateVariant(128);
            var fifthVariant = CreateVariant(129);
            var thirdVariantFailing = CreateVariant(125);
            thirdVariantFailing.Filters = "LowQ";

            // -------------------------------------------------
            // Add two variants that should be chained together
            // - When there are no neighborhoods
            // - When there is a neighborhood that they can join
            // - When there are neighborhoods available but they cannot join
            // -------------------------------------------------
            var neighborhoodManager = CreateNbhdBuilder(_sourceVcf0);
            var numRaw = 0;
            Assert.Equal(0, neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw).Count());
            Assert.Equal(0, numRaw);

            neighborhoodManager = CreateNbhdBuilder(_sourceVcf1);

            var neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(1, numRaw);
            Assert.Equal(1, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { variant, secondVariant }, neighborhoods.First());

            neighborhoodManager = CreateNbhdBuilder(_sourceVcf2);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(1, numRaw);
            Assert.Equal(1, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { variant, secondVariant, thirdVariant }, neighborhoods.First());

            neighborhoodManager = CreateNbhdBuilder(_sourceVcf3);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(2, numRaw);
            Assert.Equal(2, neighborhoods.Count());
            var neighborhood1 = neighborhoods.Last();
            Assert.True(neighborhood1.HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { variant, secondVariant, thirdVariant }, neighborhoods.First());
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { fourthVariant, fifthVariant }, neighborhood1);

            // Different phasing distance
            neighborhoodManager = CreateNbhdBuilder(_sourceVcf4, 5);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(1, numRaw);
            Assert.Equal(1, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { variant, secondVariant, fourthVariant, fifthVariant }, neighborhoods.First());

            // -------------------------------------------------
            // Passing/Failing Variants Allowed
            // -------------------------------------------------

            // Passing Only
            neighborhoodManager = CreateNbhdBuilder(_sourceVcf5);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(2, numRaw);
            Assert.Equal(2, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { variant, secondVariant }, neighborhoods.First());
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { fourthVariant, fifthVariant }, neighborhoods.Last());

            // Passing Only, Larger phasing distance
            neighborhoodManager = CreateNbhdBuilder(_sourceVcf5,5);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(1, numRaw);
            Assert.Equal(1, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { variant, secondVariant, fourthVariant, fifthVariant }, neighborhoods.First());

            // Passing Only = false
            neighborhoodManager = CreateNbhdBuilder(_sourceVcf5, passingOnly: false);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(2, numRaw);
            Assert.Equal(2, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { variant, secondVariant, thirdVariantFailing }, neighborhoods.First());
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { fourthVariant, fifthVariant }, neighborhoods.Last());

            // Passing Only = false, Larger phasing distance
            neighborhoodManager = CreateNbhdBuilder(_sourceVcf5,5, false);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(1, numRaw);
            Assert.Equal(1, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { variant, secondVariant, thirdVariantFailing, fourthVariant, fifthVariant }, neighborhoods.First());


            // -------------------------------------------------
            // Passing/Failing Variants Allowed, with min passing variants in neighborhood
            // -------------------------------------------------

            // Passing Only = false, require at least one passing var
            neighborhoodManager = CreateNbhdBuilder(_sourceVcf5, passingOnly: false, minPassingVariantsInNbhd: 1);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(2, numRaw);
            Assert.Equal(2, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { variant, secondVariant, thirdVariantFailing }, neighborhoods.First());
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { fourthVariant, fifthVariant }, neighborhoods.Last());

            // Passing Only = false, require at least two passing var
            neighborhoodManager = CreateNbhdBuilder(_sourceVcf5, passingOnly: false, minPassingVariantsInNbhd: 2);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(2, numRaw);
            Assert.Equal(2, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { variant, secondVariant, thirdVariantFailing }, neighborhoods.First());
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { fourthVariant, fifthVariant }, neighborhoods.Last());

            // Passing Only = false, require at least three passing var
            neighborhoodManager = CreateNbhdBuilder(_sourceVcf5, passingOnly: false, minPassingVariantsInNbhd: 3);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(2, numRaw);
            Assert.Equal(1, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { fourthVariant, fifthVariant }, neighborhoods.First());

            // Passing Only = false, require at least four passing var - OR be composed of all passing
            neighborhoodManager = CreateNbhdBuilder(_sourceVcf5, passingOnly: false, minPassingVariantsInNbhd: 4);
            neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(2, numRaw);
            Assert.Equal(1, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            NeighborhoodTestHelpers.CheckNeighborhoodVariants(new List<VcfVariant>() { fourthVariant, fifthVariant }, neighborhoods.First());

        }


        private NeighborhoodBuilder CreateNbhdBuilder(string sourceVcf, int phasingDistance = 2, bool passingOnly = true, int minPassingVariantsInNbhd = 0)
        {
            var variantSource = new AlleleReader(sourceVcf);

            return new NeighborhoodBuilder(
                new PhasableVariantCriteria() {ChrToProcessArray= new string[] { },PassingVariantsOnly = passingOnly, PhasingDistance=phasingDistance, MinPassingVariantsInNbhd = minPassingVariantsInNbhd}, 
                new VariantCallingParameters(), variantSource, null, 10);
        }

        private VcfVariant CreateVariant(int position)
        {
            return new VcfVariant()
            {
                ReferenceName = "chr1",
                ReferencePosition = position,
                ReferenceAllele = "A",
                VariantAlleles = new[] { "T" },
                Genotypes = new List<Dictionary<string, string>>() { new Dictionary<string, string>() { { "GT", "0/1" } } },
                InfoFields = new Dictionary<string, string>() { {"DP","1000" } },
                Filters = "PASS"
            };
        }

        [Fact]
        public void ForcedReport_alleles_are_not_included_in_neighborhood()
        {
            var sourceVcf = Path.Combine(TestPaths.LocalTestDataDirectory, "NbhdBuilderForcedGT.genome.vcf");
            var neighborhoodManager = CreateNbhdBuilder(sourceVcf,10);

            var numRaw = 0;
            var neighborhoods = neighborhoodManager.GetBatchOfCallableNeighborhoods(0, out numRaw);
            Assert.Equal(1, numRaw);
            Assert.Equal(1, neighborhoods.Count());
            Assert.True(neighborhoods.First().HasVariants);
            var neighborhood = neighborhoods.First();
            Assert.Equal(2,neighborhood.VcfVariantSites.Count);
            Assert.Equal(3751646, neighborhood.VcfVariantSites[0].VcfReferencePosition);
            Assert.Equal(3751650, neighborhood.VcfVariantSites[1].VcfReferencePosition);
            Assert.Equal("A", neighborhood.VcfVariantSites[1].VcfAlternateAllele);
        }
    }

    public static class NeighborhoodTestHelpers
    {
        public static void CheckNeighborhoodVariants(List<VcfVariant> expectedVariants, CallableNeighborhood neighborhood)
        {
            var variants = expectedVariants.Select(expectedVariant =>
                new VariantSite()
                {
                    VcfReferencePosition = expectedVariant.ReferencePosition,
                    ReferenceName = expectedVariant.ReferenceName,
                    VcfReferenceAllele = expectedVariant.ReferenceAllele,
                    VcfAlternateAllele = expectedVariant.VariantAlleles.First()
                }).ToList();
            CheckNeighborhoodVariants(variants, neighborhood);
        }

        public static void CheckNeighborhoodVariants(List<VariantSite> expectedVariantSites, CallableNeighborhood neighborhood)
        {
            Assert.Equal(expectedVariantSites.Count, neighborhood.VcfVariantSites.Count);
            foreach (var expectedVariantSite in expectedVariantSites)
            {
                Assert.True(neighborhood.VcfVariantSites.Any(v => v.ReferenceName == expectedVariantSite.ReferenceName && v.VcfReferencePosition == expectedVariantSite.VcfReferencePosition &&
                    v.VcfReferenceAllele == expectedVariantSite.VcfReferenceAllele && v.VcfAlternateAllele == expectedVariantSite.VcfAlternateAllele));
            }
        }
    }

    

}
