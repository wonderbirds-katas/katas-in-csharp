using System.Collections.Generic;
using System.Linq;

namespace TexasHoldemHands.Logic;

public static class Kata
{
    public static (string type, string[] ranks) Hand(
        string[] holeCards,
        string[] communityCards
    )
    {
        var classifier = new HandClassifierChain();
        var classification = classifier.ClassifyHand(holeCards, communityCards);

        return classification.Tuple;
    }

    private class HandClassifierChain
    {
        private readonly HandClassifier _root;

        public HandClassifierChain()
        {
            _root = new StraightFlushClassifier();
            _ = _root
                .RegisterNext(new FourOfAKindClassifier())
                .RegisterNext(new FullHouseWithTwoTriplesClassifier())
                .RegisterNext(new FullHouseWithTripleAndPairClassifier())
                .RegisterNext(new FlushClassifier())
                .RegisterNext(new StraightClassifier())
                .RegisterNext(new ThreeOfAKindClassifier())
                .RegisterNext(new ThreePairClassifier())
                .RegisterNext(new OnePairClassifier())
                .RegisterNext(new TwoPairClassifier())
                .RegisterNext(new NothingClassifier());
        }

        public HandClassification ClassifyHand(string[] holeCards, string[] communityCards) =>
            _root.ClassifyHand(new HandCards(holeCards, communityCards));
    }

    public class HandClassification
    {
        public string Type { get; set; }
        public List<string> Ranks { get; set; } = new List<string>();
        public (string type, string[] ranks) Tuple => (Type, Ranks.ToArray());
    }

    public abstract class HandClassifier
    {
        protected HandClassifier Next { get; private set; }

        public abstract HandClassification ClassifyHand(HandCards handCards);

        public HandClassifier RegisterNext(HandClassifier next)
        {
            Next = next;
            return Next;
        }
    }

    public class HandCards
    {
        public List<string> AllCards { get; }

        private List<string> RanksDescending { get; }

        public Dictionary<string, int> RankFrequencies { get; }

        public List<string> IndividualRanks { get; }

        public List<string> PairRanks { get; }

        private List<char> Suits { get; }

        public Dictionary<char, int> SuitFrequencies { get; }

        public HandCards(string[] holeCards, string[] communityCards)
        {
            AllCards = new List<string>(holeCards);
            AllCards.AddRange(communityCards);

            RanksDescending = AllCards.Select(Rank).ToList();
            RanksDescending.Sort(Descending);

            RankFrequencies = CountFrequencies(AllRanksDescending, RanksDescending);

            PairRanks = RankFrequencies
                .Where(bin => bin.Value == CardsPerPair)
                .Select(bin => bin.Key)
                .ToList();

            IndividualRanks = RankFrequencies
                .Where(bin => bin.Value == 1)
                .Select(bin => bin.Key)
                .ToList();

            Suits = AllCards.Select(Suit).ToList();

            SuitFrequencies = CountFrequencies(AllSuits, Suits);
        }

        private Dictionary<T, int> CountFrequencies<T>(IEnumerable<T> allPossibleKeys, List<T> subset)
        {
            var frequencies = allPossibleKeys.ToDictionary(key => key, _ => 0);

            subset.ForEach(key => frequencies[key]++);

            return frequencies;
        }
    }

    private class StraightFlushClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards)
        {
            var flushSuit = handCards.SuitFrequencies.FirstOrDefault(bin => bin.Value >= CardsPerHand).Key;
            var isFlush = flushSuit != 0;

            var flushRanks = handCards.AllCards
                .Where(card => Suit(card) == flushSuit)
                .Select(Rank)
                .OrderBy(OrdinalNumberOf)
                .ToHashSet();

            var (startIndex, length) = StraightHelper.FindConsecutiveCards(flushRanks);
            var isStraight = length >= StraightHelper.RequiredNumberOfConsecutiveCards;

            if (!isFlush || !isStraight)
            {
                return Next.ClassifyHand(handCards);
            }

            var handClassification = new HandClassification();
            handClassification.Type = StraightFlush;
            handClassification.Ranks.AddRange(flushRanks.Skip(startIndex).Take(CardsPerHand));

            return handClassification;
        }
    }

    private static class StraightHelper
    {
        public const int RequiredNumberOfConsecutiveCards = 4;

        public static (int startIndex, int length) FindConsecutiveCards(IEnumerable<string> rankSet)
        {
            var ordinalNumbers = rankSet.Select(OrdinalNumberOf).ToList();
            var countConsecutiveCards = 0;
            var currentIndex = 1;

            while (countConsecutiveCards < RequiredNumberOfConsecutiveCards && currentIndex < ordinalNumbers.Count)
            {
                if (ordinalNumbers[currentIndex - 1] + 1 == ordinalNumbers[currentIndex])
                {
                    countConsecutiveCards++;
                }
                else
                {
                    countConsecutiveCards = 0;
                }

                currentIndex++;
            }

            return (currentIndex - countConsecutiveCards - 1, countConsecutiveCards);
        }
    }

    private class FourOfAKindClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards)
        {
            var rank = handCards.RankFrequencies.FirstOrDefault(bin => bin.Value == 4).Key;

            var isFourOfAKind = !string.IsNullOrEmpty(rank);
            if (!isFourOfAKind)
            {
                return Next.ClassifyHand(handCards);
            }

            var handClassification = new HandClassification();
            handClassification.Type = FourOfAKind;
            handClassification.Ranks.Add(rank);
            handClassification.Ranks.Add(handCards.RankFrequencies.First(bin => 0 < bin.Value && bin.Value < 4).Key);
            return handClassification;
        }
    }

    private class FullHouseWithTwoTriplesClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards)
        {
            var tripleRanks = handCards.RankFrequencies
                .Where(bin => bin.Value == CardsPerTriple)
                .Select(bin => bin.Key)
                .OrderBy(OrdinalNumberOf)
                .ToList();

            var isFullHouse = tripleRanks.Count == 2;

            if (!isFullHouse)
            {
                return Next.ClassifyHand(handCards);
            }

            var handClassification = new HandClassification();
            handClassification.Type = FullHouse;
            handClassification.Ranks.AddRange(tripleRanks);

            return handClassification;
        }
    }

    private class FullHouseWithTripleAndPairClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards)
        {
            var pairRank = handCards.RankFrequencies.FirstOrDefault(bin => bin.Value == CardsPerPair).Key;
            var tripleRank = handCards.RankFrequencies.FirstOrDefault(bin => bin.Value == CardsPerTriple).Key;
            var isFullHouse = !string.IsNullOrEmpty(pairRank) && !string.IsNullOrEmpty(tripleRank);

            if (!isFullHouse)
            {
                return Next.ClassifyHand(handCards);
            }

            var handClassification = new HandClassification();

            handClassification.Type = FullHouse;
            handClassification.Ranks.Add(tripleRank);
            handClassification.Ranks.Add(pairRank);

            return handClassification;
        }
    }

    public class FlushClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards)
        {
            var flushSuit = handCards.SuitFrequencies.FirstOrDefault(bin => bin.Value >= CardsPerHand).Key;
            var isFlush = flushSuit != 0;

            if (!isFlush)
            {
                return Next.ClassifyHand(handCards);
            }

            var ranks = handCards.AllCards.Where(card => Suit(card) == flushSuit).Select(Rank).ToList();
            ranks.Sort(Descending);

            var handClassification = new HandClassification();
            handClassification.Type = Flush;
            handClassification.Ranks.AddRange(ranks.Take(CardsPerHand));

            return handClassification;
        }
    }

    private class StraightClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards)
        {
            var rankSet = handCards.RankFrequencies
                .Where(bin => bin.Value > 0)
                .Select(bin => bin.Key)
                .ToList();

            var (startIndex, length) = StraightHelper.FindConsecutiveCards(rankSet);

            if (length < StraightHelper.RequiredNumberOfConsecutiveCards)
            {
                return Next.ClassifyHand(handCards);
            }

            var handClassification = new HandClassification();
            handClassification.Type = Straight;
            handClassification.Ranks.AddRange(rankSet.Skip(startIndex).Take(CardsPerHand));

            return handClassification;
        }
    }

    private class ThreeOfAKindClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards)
        {
            var tripleRank = handCards.RankFrequencies.FirstOrDefault(IsTriple).Key;
            var isThreeOfAKind = !string.IsNullOrEmpty(tripleRank);

            if (!isThreeOfAKind)
            {
                return Next.ClassifyHand(handCards);
            }

            var handClassification = new HandClassification();
            handClassification.Type = ThreeOfAKind;
            handClassification.Ranks.Add(tripleRank);
            handClassification.Ranks.AddRange(handCards.IndividualRanks.Take(CardsPerHand - CardsPerTriple));

            return handClassification;
        }

        private bool IsTriple(KeyValuePair<string, int> cardAndQuantity) => cardAndQuantity.Value == CardsPerTriple;
    }

    private class ThreePairClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards)
        {
            if (handCards.PairRanks.Count != 3)
            {
                return Next.ClassifyHand(handCards);
            }

            var remainingCards = new List<string>(handCards.IndividualRanks);
            remainingCards.AddRange(handCards.PairRanks.Skip(2));

            var handClassification = new HandClassification();
            handClassification.Type = TwoPair;
            handClassification.Ranks.AddRange(handCards.PairRanks.Take(2));
            handClassification.Ranks.AddRange(remainingCards.Take(CardsPerHand - 2 * CardsPerPair));

            return handClassification;
        }
    }

    private class TwoPairClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards)
        {
            if (handCards.PairRanks.Count != 2)
            {
                return Next.ClassifyHand(handCards);
            }

            var handClassification = new HandClassification();
            handClassification.Type = TwoPair;
            handClassification.Ranks.AddRange(handCards.PairRanks.Take(2));
            handClassification.Ranks.AddRange(handCards.IndividualRanks.Take(CardsPerHand - 2 * CardsPerPair));

            return handClassification;
        }
    }

    private class OnePairClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards)
        {
            if (handCards.PairRanks.Count != 1)
            {
                return Next.ClassifyHand(handCards);
            }

            var handClassification = new HandClassification();
            handClassification.Type = Pair;
            handClassification.Ranks.Add(handCards.PairRanks.First());
            handClassification.Ranks.AddRange(handCards.IndividualRanks.Take(CardsPerHand - CardsPerPair));

            return handClassification;
        }
    }

    private class NothingClassifier : HandClassifier
    {
        public override HandClassification ClassifyHand(HandCards handCards) =>
            new HandClassification() { Type = Nothing, Ranks = handCards.IndividualRanks.Take(CardsPerHand).ToList() };
    }

    private const string Nothing = "nothing";
    private const string Pair = "pair";
    private const string TwoPair = "two pair";
    private const string ThreeOfAKind = "three-of-a-kind";
    private const string Straight = "straight";
    private const string Flush = "flush";
    private const string FullHouse = "full house";
    private const string FourOfAKind = "four-of-a-kind";
    private const string StraightFlush = "straight-flush";

    private const int CardsPerHand = 5;
    private const int CardsPerPair = 2;
    private const int CardsPerTriple = 3;

    private static readonly List<string> AllRanksDescending = new List<string>()
    {
        "A",
        "K",
        "Q",
        "J",
        "10",
        "9",
        "8",
        "7",
        "6",
        "5",
        "4",
        "3",
        "2"
    };

    private static readonly char[] AllSuits = { '♠', '♦', '♣', '♥' };

    private static int OrdinalNumberOf(string rank) => AllRanksDescending.IndexOf(rank);

    private static int Descending(string x, string y)
    {
        var xIndex = AllRanksDescending.IndexOf(x);
        var yIndex = AllRanksDescending.IndexOf(y);

        return xIndex < yIndex ? -1 : 1;
    }

    private static string Rank(string card) => card[..^1];

    private static char Suit(string card) => card[^1];
}