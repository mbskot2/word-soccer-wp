﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WordSoccer.Game.Games;

namespace WordSoccer.Game.Players
{
	public class Player : IPlayer
	{
		private readonly String name;
		private readonly List<Word> words;
		private readonly List<Card> cards;
		private readonly Letter[] letters;
		private int points, totalPoints, score, usedLetters, redCards, yellowCards;
		private IGame game;
		private IPlayerListener listener;

		public Player(String name)
		{
			this.name = name;
			this.words = new List<Word>();
			this.cards = new List<Card>();
			this.letters = new Letter[BaseGame.LETTERS];
		}

		public String GetName()
		{
			return name;
		}

		public int GetScore()
		{
			return score;
		}

		public void SetScore(int score)
		{
			this.score = score;
		}

		public async void AddWord(Word word)
		{
			if (game == null)
			{
				throw new Exception("Game is not started properly.");
			}

			words.Add(word);
			words.Sort();

			if (listener != null)
			{
				listener.OnWordAdded(word);
			}

			bool valid = await Task.Run(() => game.GetDictionary().IsWordValid(word.word));

			if (valid)
			{
				word.SetState(Word.WordState.VALID);

				AddUsedLetters(word);
				points += word.word.Length;
				totalPoints += word.word.Length;
			}
			else
			{
				word.SetState(Word.WordState.INVALID);
			}
			
			words.Sort();
		}

		public List<Word> GetWords()
		{
			return words;
		}

		public int GetCurrentLongestWord()
		{
			int length = 0;

			foreach (Word word in words)
			{
				if (word.GetState() == Word.WordState.VALID && word.word.Length > length)
				{
					length = word.word.Length;
				}
			}

			return length;
		}

		public int GetPoints()
		{
			return points;
		}

		public void ResetPoints()
		{
			points = 0;
		}

		public int GetTotalPoints()
		{
			return totalPoints;
		}

		public Letter[] GetLetters()
		{
			return letters;
		}

		public bool HasUsedAllLetters()
		{
			return usedLetters == BaseGame.LETTERS;
		}

		public int GetNumberOfUsedLetters()
		{
			return usedLetters;
		}

		public void AddCard(Card card)
		{
			if (GetNumberOfCards(Card.CardType.RED) + 1 > BaseGame.MAX_RED_CARDS)
			{
				return;
			}

			cards.Add(card);

			int indexOfLastEnableLetter = letters.Length - 1 - GetNumberOfCards(Card.CardType.RED);
			Letter letter = letters[indexOfLastEnableLetter];

			if (card.GetCardType() == Card.CardType.YELLOW)
			{
				letter.SetCardType(letter.GetCardType() == Card.CardType.YELLOW
					? Card.CardType.RED : Card.CardType.YELLOW);
				yellowCards++;
			}
			else
			{
				if (letter.GetCardType() == Card.CardType.YELLOW)
				{
					Letter prevLetter = letters[indexOfLastEnableLetter - 1];
					prevLetter.SetCardType(Card.CardType.YELLOW);
				}

				letter.SetCardType(Card.CardType.RED);
				redCards++;
			}
		}

		public List<Card> GetCards()
		{
			return cards;
		}

		public int GetNumberOfCards(Card.CardType cardType)
		{
			return cardType == Card.CardType.YELLOW
				? yellowCards % 2
				: yellowCards / 2 + redCards;
		}

		public void OnStartGame(IGame game)
		{
			this.game = game;
			score = points = totalPoints = usedLetters = redCards = yellowCards = 0;

			words.Clear();
			cards.Clear();

			for (int i = 0; i < letters.Length; i++)
			{
				letters[i] = new Letter(i);
			}
		}

		public void OnStartRound(IGame game)
		{
			// reset used letters
			for (int i = 0; i < letters.Length; i++)
			{
				letters[i].SetSign(game.GetCurrentRoundLetters()[i])
					.SetUsed(false);
			}

			usedLetters = 0;

			// reset found words
			words.Clear();
		}

		public void SetListener(IPlayerListener listener)
		{
			this.listener = listener;
		}

		private void AddUsedLetters(Word word)
		{
			for (int i = 0; i < word.word.Length; i++)
			{
				foreach (Letter letter in letters)
				{
					if (!letter.IsUsed() && !letter.IsDisabled() && word.word[i] == letter.GetSign())
					{
						letter.SetUsed(true);
						usedLetters++;
						break;
					}
				}
			}
		}
	}
}