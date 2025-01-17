# MTCG_SWEN1

# MTCG Project - Final Submission Protocol

**Author:** Kenn-Michael Sanga  
**Student ID:** if23b128  
**GitHub Repository:** [MTCG_SWEN1](https://github.com/kenno10101/MTCG_SWEN1)

---

## Overview

This project is a **Monster Trading Card Game (MTCG)** implementation featuring a RESTful API, battle logic, and database integration. The game allows players to collect cards, build decks, trade cards, and engage in battles.

---

## Key Implementation Decisions

1. **Card Interface**

   - `Monstercard` and `Spellcard` inherit from a shared `Card` interface.
   - All cards are stored in a list of the `Card` interface.

2. **Battle System**

   - Battles are conducted between two players using a `ConcurrentQueue` to manage battle requests.
   - Responses are synchronized for both clients.

3. **Database Design**

   - Tables implemented:
     - `BattleHistory`: Logs all battles.
     - `Cards`: Stores all available cards.
     - `Deck`: Stores users' decks.
     - `Packages`: Stores card packages.
     - `Sessions`: Handles user sessions.
     - `Stack`: Manages users' cards.
     - `Stats`: Tracks user stats and scores.
     - `Tradings`: Records trade deals.
     - `Users`: Stores user information.
   - Separate database used for unit tests.

4. **Enums**
   - Card Types: `Monster`, `Spell`
   - Elements: `Water`, `Fire`, `Normal`
   - Monsters: `Null`, `Goblin`, `Dragon`, `Wizard`, `Ork`, `Knight`, `Kraken`, `Elf`
   - Trading Status: `Open`, `Done`, `Deleted`

---

## Project Structure

- **Handlers**  
  Define routes and format responses:

  - `BattleHandler`
  - `CardHandler`
  - `SessionHandler`
  - `TradingHandler`
  - `UserHandler`

- **Interfaces**  
  Define shared interfaces for cards and handlers.

- **Misc**  
  Enums and helper functions.

- **Models**  
  Essential classes:

  - `Battle`, `Card`, `Stats`, `Tradings`, `Users`

- **Network**  
  Handles request parsing and database connections.

- **Repositories**  
  Database query functions for transactions.

- **Specs**  
  Unit tests for `User`, `Package`, `Deck`, and `Battle`.

---

## Features

### User Management

- **POST /users**: Register user.
- **GET /users/:username**: View user information.
- **PUT /users/:username**: Update user details.

### Session Management

- **POST /sessions**: User login.

### Card Management

- **GET /cards**: List all owned cards.

### Deck Management

- **GET /deck**: View configured deck.
- **PUT /deck**: Configure deck.

### Battle System

- **POST /battle**: Initiate a battle between two users.

### Statistics

- **GET /stats**: View battle statistics.
- **GET /scoreboard**: View global leaderboard.

### Trading System

- **GET /tradings**: View open trades.
- **POST /tradings**: Create a trade deal.
- **POST /tradings/:id**: Accept a trade.
- **DELETE /tradings**: Delete a trade.

---

## Unique Features

- **Battle Rank System**:  
  Players are ranked by Elo score.
  - **Bronze**: <110
  - **Silver**: 110-125
  - **Gold**: 125-140
  - **Platinum**: 140-155
  - **Diamond**: 155-170
  - **Master**: >170

---

## Lessons Learned

1. **Unit Testing with NUnit**
   - Verified correctness and identified edge cases.
2. **RESTful API Development**
   - Enhanced understanding of low-level HTTP operations.
3. **Database Integration**
   - Developed proficiency in PostgreSQL with C#.

---

## Development Timeline

| Task                      | Hours | Notes                                  |
| ------------------------- | ----- | -------------------------------------- |
| Initial Setup             | 1     | Environment and dependencies setup.    |
| Model Design              | 1     | UML Class Design.                      |
| Model Implementation      | 5     | Structuring classes and project.       |
| Routes & Handlers         | 10    | API endpoints coding and testing.      |
| Repository Implementation | 8     | Database queries and transactions.     |
| Database Design           | 5     | Table creation and enums.              |
| Unit Tests                | 2     | Writing tests for key functionalities. |
| Integration Tests         | 2     | Testing with curl and Postman.         |
| Documentation             | 2     | Finalizing project documentation.      |

**Total Time Spent:** 36 hours

---

## Testing

- **Unit Tests**: Separate testing environment database.
- **Integration Testing**: Postman collection included (`MTCG Project.postman_collection.json`).

---

Check out the project on GitHub: [MTCG_SWEN1](https://github.com/kenno10101/MTCG_SWEN1).
