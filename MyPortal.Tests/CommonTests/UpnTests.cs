using MyPortal.Common.Identifiers;

namespace MyPortal.Tests.CommonTests
{
    [TestFixture]
    public class UpnTests
    {
        [Test]
        public void CheckLetter_AllZeros_IsA()
        {
            Assert.That(Upn.CheckLetter("000000000000"), Is.EqualTo('A'));
        }

        [Test]
        public void CheckLetter_TrailingOne_IsP()
        {
            // Only the final significant character contributes: 1 * weight 13 = 13; 13 mod 23 -> 'P'.
            Assert.That(Upn.CheckLetter("000000000001"), Is.EqualTo('P'));
        }

        [Test]
        public void Compose_KnownParts_ProducesExpectedUpn()
        {
            // LA 202, establishment 1234, year 2026, serial 7 -> significant 202123426007,
            // weighted sum 251, 251 mod 23 = 21 -> 'Y'.
            Assert.That(Upn.Compose(202, 1234, 2026, 7), Is.EqualTo("Y202123426007"));
        }

        [Test]
        public void Compose_UsesLastTwoDigitsOfYearAndPadsParts()
        {
            var upn = Upn.Compose(1, 23, 2007, 4);

            Assert.That(upn.Length, Is.EqualTo(13));
            Assert.That(upn[1..], Is.EqualTo("001002307004"));
        }

        [Test]
        public void Compose_RoundTripsThroughIsValid()
        {
            foreach (var serial in new[] { 0, 1, 42, 999 })
            {
                Assert.That(Upn.IsValid(Upn.Compose(878, 4001, 2026, serial)), Is.True);
            }
        }

        [Test]
        public void Compose_SerialOutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Upn.Compose(202, 1234, 2026, 1000));
        }

        [Test]
        public void IsValid_CorrectPermanentUpn_IsTrue()
        {
            Assert.That(Upn.IsValid("Y202123426007"), Is.True);
        }

        [Test]
        public void IsValid_WrongCheckLetter_IsFalse()
        {
            Assert.That(Upn.IsValid("B000000000000"), Is.False);
        }

        [Test]
        public void IsValid_ExcludedLetterInCheckPosition_IsFalse()
        {
            Assert.That(Upn.IsValid("I000000000000"), Is.False);
        }

        [Test]
        public void IsValid_WellFormedTemporaryUpn_IsTrue()
        {
            // Alphabetic serial in the final position -> temporary; accepted as well-formed.
            Assert.That(Upn.IsValid("A00000000000A"), Is.True);
        }

        [Test]
        public void IsValid_NonDigitInSignificantBody_IsFalse()
        {
            Assert.That(Upn.IsValid("A0000000000A0"), Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("A00000000000")]
        [TestCase("A0000000000000")]
        public void IsValid_WrongLength_IsFalse(string? upn)
        {
            Assert.That(Upn.IsValid(upn), Is.False);
        }
    }
}
