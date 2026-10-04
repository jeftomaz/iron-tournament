using System;
using NUnit.Framework;

namespace IronTournament.Core.Tests
{
    public sealed class RandomSourceTests
    {
        [Test]
        public void SameSeedProducesTheSameSequence()
        {
            var first = new SeededRandomSource(42);
            var second = new SeededRandomSource(42);

            for (var index = 0; index < 100; index++)
            {
                Assert.That(first.Next(0, 100), Is.EqualTo(second.Next(0, 100)));
            }
        }

        [Test]
        public void NextStaysWithinTheRequestedRange()
        {
            var random = new SeededRandomSource(7);

            for (var index = 0; index < 1000; index++)
            {
                Assert.That(random.Next(-3, 4), Is.InRange(-3, 3));
            }
        }

        [TestCase(5, 5)]
        [TestCase(6, 5)]
        public void NextRejectsAnEmptyRange(int minInclusive, int maxExclusive)
        {
            var random = new SeededRandomSource(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => random.Next(minInclusive, maxExclusive));
        }
    }
}
