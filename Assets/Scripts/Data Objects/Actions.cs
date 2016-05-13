using UnityEngine;
using CharacterObjects;
using CivObjects;
using ConversationAI;
using EconomicObjects;
using StellarObjects;
using HelperFunctions;
using System.Collections.Generic;

namespace Actions
{
    public class CharacterAction
    {
        public enum eType : int
        {
            Political,
            Military,
            Economic,
            Personal,
            Psychic,
            IntelOps
        }

        public string Name { get; set; }
        public string ID { get; set; }
        public eType Category { get; set; }
        public string Description { get; set; }
        public bool ViceroyValid { get; set; }
        public bool SysGovValid { get; set; }
        public bool ProvGovValid { get; set; }
        public bool PrimeValid { get; set; } // need to add all the other Prime positions
        public bool AllValid { get; set; }
        public bool InquisitorValid { get; set; }
        public bool EmperorNearValid { get; set; }
        public bool EmperorAction { get; set; }

        public bool IsActionValid(Character cData, Civilization civ)
        {
            if (AllValid)
            {
                return true;
            }

            if (cData.Role == Character.eRole.Emperor && !EmperorAction) // if not an Emperor character action, kick
                return false;

            if (cData.Role == Character.eRole.Viceroy && !ViceroyValid)
            {
                return false;
            }

            if (cData.Role == Character.eRole.SystemGovernor && !SysGovValid)
            {
                return false;
            }

            if (cData.Role == Character.eRole.ProvinceGovernor && !ProvGovValid)
            {
                return false;
            }

            if (cData.Role == Character.eRole.DomesticPrime && !PrimeValid)
            {
                return false;
            }

            if (cData.Role == Character.eRole.Inquisitor && !InquisitorValid)
            {
                return false;
            }

            if (EmperorNearValid)
            {
                if (civ.Leader != null)
                {
                    if (cData.PlanetLocationID != civ.Leader.PlanetLocationID)
                    {
                        return false;
                    }

                }
                else
                {
                    return false; // no leader, so false anyway
                }
            }
            return true; // passed all tests so it's true


        }

    }

    // action functions
    public static class ActionFunctions
    {

        // relationship/action helper functions (create challenges, etc)
        public static string CreateChallenge(string challengerCharID, string challengedCharID, int influenceStaked)
        {
            GameData gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            Challenge newChallenge = new Challenge();

            // create a unique ID until it does not exist in any other challenge (highly unlikely, but still)
            do
                newChallenge.ChallengeID = Random.Range(0, 999999).ToString("N0") + (char)Random.Range(0, 25); // create a unique ID;
            while (gDataRef.ChallengeList.Exists(p => p.ChallengeID == newChallenge.ChallengeID));

            newChallenge.CharacterChallengerID = challengerCharID;
            newChallenge.CharacterChallengedID = challengedCharID;
            newChallenge.InfluenceStakedOnChallenge += influenceStaked;
            newChallenge.TurnsSinceChallengeIssued = 0; // new challenge, no turns elapsed
            gDataRef.ChallengeList.Add(newChallenge); // add the challenge to the global list

            return newChallenge.ChallengeID; // returns the reference ID
        }

        public static void ResolveChallenge(Challenge activeChallenge, string winningCharacterID, string losingCharacterID)
        {
            GameData gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            activeChallenge.WinnerOfChallengeID = winningCharacterID;
            activeChallenge.LoserOfChallengeID = losingCharacterID;

            // adjust influence gained/lost depending on challenge result
            HelperFunctions.DataRetrivalFunctions.GetCharacter(activeChallenge.WinnerOfChallengeID).BaseInfluence += activeChallenge.InfluenceStakedOnChallenge;
            HelperFunctions.DataRetrivalFunctions.GetCharacter(activeChallenge.LoserOfChallengeID).BaseInfluence -= activeChallenge.InfluenceStakedOnChallenge;

            // change the relationship states of both characters
            HelperFunctions.DataRetrivalFunctions.GetCharacter(activeChallenge.WinnerOfChallengeID).Relationships[losingCharacterID].RelationshipState = Relationship.eRelationshipState.Vendetta;
            HelperFunctions.DataRetrivalFunctions.GetCharacter(activeChallenge.LoserOfChallengeID).Relationships[winningCharacterID].RelationshipState = Relationship.eRelationshipState.Vendetta;

            // probably should add debug output, log output, or event code here, remove the challenge from the active challenge list
            gDataRef.ChallengeList.Remove(activeChallenge);
        }

        public static void CheckRelationshipStatus(Character xChar, Character yChar)
        {
            Relationship.eRelationshipState relStateX = xChar.Relationships[yChar.ID].RelationshipState; // get the relationship
            Relationship.eRelationshipState relStateY = xChar.Relationships[xChar.ID].RelationshipState; // get the relationship

            if (xChar.Role > yChar.Role)
            {
                relStateX = Relationship.eRelationshipState.Superior;
                relStateY = Relationship.eRelationshipState.Inferior;
            }

            if (xChar.Role < yChar.Role)
            {
                relStateX = Relationship.eRelationshipState.Inferior;
                relStateY = Relationship.eRelationshipState.Superior;
            }
        }
        public static string GivePraisingSpeech(Character cData)
        {
            GameData gDataRef = GameObject.Find("GameManager").GetComponent<GameData>(); 
            Character eData = gDataRef.CivList[0].PlayerEmperor; // you
            CharacterAction aData = gDataRef.CharacterActionList.Find(p => p.ID == "A1");
            
            int speechEffectiveness = 0;
            string conversationFlags = "";
            string speechSuccess = ""; // debug code

            // now determine effect of character          
            conversationFlags += "[PLEASED]";

            if (cData.Relationships[eData.ID].Trust > 50)
            {
                cData.Relationships[eData.ID].Trust += Random.Range(0, (speechEffectiveness / 5));
            }
            else
            {
                cData.Relationships[eData.ID].Trust += Random.Range(0, (speechEffectiveness / 8)); // less effective when more hated
            }

            // now determine effect of characters around them, checking each character individually
            foreach (string cID in cData.Relationships.Keys)
            {
                if (cData.Relationships.ContainsKey(cID)) // need a check here for the emperor
                {
                    if (cData.Relationships[cID].RelationshipState == Relationship.eRelationshipState.Friends || cData.Relationships[cID].RelationshipState == Relationship.eRelationshipState.Allies)
                    {
                        cData.Relationships[cID].Trust += Random.Range(0, (speechEffectiveness / 8));
                       // DataRetrivalFunctions.GetCharacter(cID).Relationships[eData.ID].Trust += Random.Range(0, (speechEffectiveness / 10)); // improve trust slightly with the emperor
                    }

                    if (cData.Relationships[cID].RelationshipState == Relationship.eRelationshipState.Rival || cData.Relationships[cID].RelationshipState == Relationship.eRelationshipState.Vendetta)
                    {
                        cData.Relationships[cID].Trust -= Random.Range(0, (speechEffectiveness / 8));
                       // DataRetrivalFunctions.GetCharacter(cID).Relationships[eData.ID].Trust -= Random.Range(0, (speechEffectiveness / 10)); // distrusts slightly with the emperor
                    }

                    if (cData.Relationships[cID].RelationshipState == Relationship.eRelationshipState.SwornVengeance)
                    {
                        cData.Relationships[cID].Trust -= Random.Range(0, (speechEffectiveness / 6));
                       // DataRetrivalFunctions.GetCharacter(cID).Relationships[eData.ID].Trust -= Random.Range(0, (speechEffectiveness / 6)); // distrusts a lot with the emperor
                    }
                }
            }         

            // now send to speech engine to create response and return response
            UnityEngine.Debug.Log("Give Praising Speech executed. Speech was " + speechSuccess); // debug code
            string response = "ACTION CHECK VALUE: " + speechEffectiveness.ToString("N0") + " " + ConversationEngine.GenerateResponse(cData, aData, 100, false, conversationFlags);

            return response;
        }

        public static string GivePublicReprimand(Character reprimandingChar, Character reprimandedChar)
        {
            GameData gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            CharacterAction aData = gDataRef.CharacterActionList.Find(p => p.ID == "A2");
            Relationship firstCharacterInitialState = reprimandingChar.Relationships[reprimandedChar.ID];
            Relationship secondCharacterInitialState = reprimandedChar.Relationships[reprimandingChar.ID];
            string conversationFlags = ""; // flags that are added to the conversation
            float speechEffectiveness = 0;

            // code to change the relationships here
            conversationFlags += "[HATE]";

            speechEffectiveness = Random.Range(30f, reprimandingChar.Charm) + Random.Range(0, reprimandingChar.Intelligence);

            if (reprimandedChar.Relationships[reprimandingChar.ID].Trust > 50)
            {
                reprimandedChar.Relationships[reprimandingChar.ID].Trust -= Random.Range(0, (speechEffectiveness / 5));
            }
            else
            {
                reprimandedChar.Relationships[reprimandingChar.ID].Trust -= Random.Range(0, (speechEffectiveness / 8)); // less effective when more hated
            }

            // now determine effect of characters around them, checking each character individually
            foreach (string cID in reprimandedChar.Relationships.Keys)
            {
                if (reprimandedChar.Relationships.ContainsKey(cID))
                {
                    if (reprimandedChar.Relationships[cID].RelationshipState == Relationship.eRelationshipState.Friends || reprimandedChar.Relationships[cID].RelationshipState == Relationship.eRelationshipState.Allies)
                    {
                        reprimandedChar.Relationships[cID].Trust += Random.Range(0, (speechEffectiveness / 8));
                        HelperFunctions.DataRetrivalFunctions.GetCharacter(cID).Relationships[reprimandingChar.ID].Trust -= Random.Range(0, (speechEffectiveness / 10)); // improve trust slightly with the emperor
                    }

                    if (reprimandedChar.Relationships[cID].RelationshipState == Relationship.eRelationshipState.Rival || reprimandedChar.Relationships[cID].RelationshipState == Relationship.eRelationshipState.Vendetta)
                    {
                        reprimandedChar.Relationships[cID].Trust -= Random.Range(0, (speechEffectiveness / 8));
                        HelperFunctions.DataRetrivalFunctions.GetCharacter(cID).Relationships[reprimandingChar.ID].Trust += Random.Range(0, (speechEffectiveness / 10)); // distrusts slightly with the emperor
                    }

                    if (reprimandedChar.Relationships[cID].RelationshipState == Relationship.eRelationshipState.SwornVengeance)
                    {
                        reprimandedChar.Relationships[cID].Trust -= Random.Range(0, (speechEffectiveness / 6));
                        HelperFunctions.DataRetrivalFunctions.GetCharacter(cID).Relationships[reprimandingChar.ID].Trust += Random.Range(0, (speechEffectiveness / 6)); // distrusts a lot with the emperor
                    }
                }
            }

            // add code to check house responses here

            string response = "EFFECTIVENESS: " + speechEffectiveness.ToString("N0") + " " + ConversationEngine.GenerateResponse(reprimandedChar, aData, 0, false, conversationFlags);
            return response;
        }

        public static string OrderToChangeExport(Character target, Character initiator, Trade.eTradeGood currentExport, Trade.eTradeGood desiredExport)
        {
            GameData gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            Character eData = gDataRef.CivList[0].PlayerEmperor; // you
            CharacterAction aData = gDataRef.CharacterActionList.Find(p => p.ID == "A6"); // pull the action data from the data list
            string actionResult = "";
            string conversationFlags = ""; // flags to send along for the 
            string response = ""; // the response.
            float currentExportProfit = 0f;
            float newExportProfit = 0f;           

            // inputs
            List<Trade.eTradeGood> tradeGoodsExported = new List<Trade.eTradeGood>();
            List<Trade.eTradeGood> tradeGoodsImported = new List<Trade.eTradeGood>();
            PlanetData targetPlanet = DataRetrivalFunctions.GetPlanet(target.PlanetAssignedID);

            // build the list of exports from a certain planet (may actually create a function for this within PlanetData)
            switch (currentExport)
            {
                case Trade.eTradeGood.Food:
                    currentExportProfit = targetPlanet.FoodExported * targetPlanet.Owner.CurrentFoodPrice;
                    break;
                case Trade.eTradeGood.Energy:
                    currentExportProfit = targetPlanet.EnergyExported * targetPlanet.Owner.CurrentEnergyPrice;
                    break;
                case Trade.eTradeGood.Basic:
                    currentExportProfit = targetPlanet.AlphaExported * targetPlanet.Owner.CurrentBasicPrice;
                    break;
                case Trade.eTradeGood.Heavy:
                    currentExportProfit = targetPlanet.HeavyExported * targetPlanet.Owner.CurrentHeavyPrice;
                    break;
                case Trade.eTradeGood.Rare:
                    currentExportProfit = targetPlanet.RareExported * targetPlanet.Owner.CurrentRarePrice;
                    break;
                default:
                    break;
            }

            switch (desiredExport)
            {
                case Trade.eTradeGood.Food:
                    newExportProfit = targetPlanet.FoodExported * targetPlanet.Owner.CurrentFoodPrice;
                    break;
                case Trade.eTradeGood.Energy:
                    newExportProfit = targetPlanet.EnergyExported * targetPlanet.Owner.CurrentEnergyPrice;
                    break;
                case Trade.eTradeGood.Basic:
                    newExportProfit = targetPlanet.AlphaExported * targetPlanet.Owner.CurrentBasicPrice;
                    break;
                case Trade.eTradeGood.Heavy:
                    newExportProfit = targetPlanet.HeavyExported * targetPlanet.Owner.CurrentHeavyPrice;
                    break;
                case Trade.eTradeGood.Rare:
                    newExportProfit = targetPlanet.RareExported * targetPlanet.Owner.CurrentRarePrice;
                    break;
                default:
                    break;
            }

            // build the list of imports - is this needed?

            // decide whether to change or not
            float obeyDecisionWeight = 0f;
            float defyDecisionWeight = 0f;
            float profitChange = newExportProfit/currentExportProfit;

            obeyDecisionWeight += 100 + (profitChange * 100);
            defyDecisionWeight += 20; // need some answers with this one

            if (obeyDecisionWeight > defyDecisionWeight)
            {
                actionResult = "ORDER OBEYED. CHANGING EXPORT TO " + desiredExport.ToString() + " ";
                conversationFlags += "[PLEASED]";
                response += actionResult + ConversationEngine.GenerateResponse(target, aData, 100, false, conversationFlags);
                TradeProposal tP = new TradeProposal();
                tP.TradeResource = Trade.eTradeGood.Energy;
                target.PlanetAssigned.ActiveTradeProposalList.Add(tP);
            }
            else
            {
                actionResult = "ORDER DISOBEYED ";
                conversationFlags += "[HATE]";
                response += actionResult + ConversationEngine.GenerateResponse(target, aData, 0, false, conversationFlags);
            }

            return response;
        }
  
        public static string IssueInsultToCharacter(Character challengingChar, Character insultedChar)
        {
            GameData gDataRef = GameObject.Find("GameManager").GetComponent<GameData>();
            Character eData = gDataRef.CivList[0].PlayerEmperor; // you
            CharacterAction aData = gDataRef.CharacterActionList.Find(p => p.ID == "A3");
            Relationship firstCharacterInitialState = insultedChar.Relationships[challengingChar.ID];
            Relationship secondCharacterInitialState = challengingChar.Relationships[insultedChar.ID];
            string activeChallengeID = ""; // current challenge ID for accessor
            string actionResult = ""; // shows result in string (debug)
            string conversationFlags = ""; // flags that are added to the conversation

            if ((firstCharacterInitialState.RelationshipState != Relationship.eRelationshipState.Vendetta) && (firstCharacterInitialState.RelationshipState != Relationship.eRelationshipState.Predator))
            {
                // create a challenge!               
                activeChallengeID = CreateChallenge(challengingChar.ID, insultedChar.ID, Random.Range(0, challengingChar.Power / 10));
                secondCharacterInitialState.GrudgeLevel += .1f; // increase the grudge level
                actionResult += "CHALLENGE ISSUED!";
                conversationFlags += "[HATE]";
                firstCharacterInitialState.RelationshipState = Relationship.eRelationshipState.Challenger; // change initial relationship
            }

            if ((secondCharacterInitialState.RelationshipState == Relationship.eRelationshipState.Lovers))
            {
                // create test to see if love ends here
                //
                challengingChar.Relationships[insultedChar.ID].RomanticLove = false; // love is gone!
                secondCharacterInitialState.BetrayalLevel = 1f; // betrayed!
                actionResult += "BETRAYAL!";
                conversationFlags += "[BETRAY]";
                challengingChar.Honor -= 10; // honor hit
                secondCharacterInitialState.GrudgeLevel += .3f; // increase grudge 30%
            }

            if (secondCharacterInitialState.RelationshipState == Relationship.eRelationshipState.Allies)
            {
                if (challengingChar.Honor > 80)
                    challengingChar.Honor = 80; // set honor to 80 if above, no change if below that
            }

            if (secondCharacterInitialState.RelationshipState == Relationship.eRelationshipState.Friends)
            {
                secondCharacterInitialState.BetrayalLevel = 1f; // betrayed!
                challengingChar.Honor -= 10; // honor hit
            }

            if (secondCharacterInitialState.RelationshipState == Relationship.eRelationshipState.Superior || secondCharacterInitialState.RelationshipState == Relationship.eRelationshipState.HungUpon)
            {
                Challenge curChallenge;
                List<Challenge> chalList = gDataRef.ChallengeList.FindAll(p => p.CharacterChallengedID == insultedChar.ID);
                if (chalList.Exists(p => p.CharacterChallengerID == challengingChar.ID))
                {
                    curChallenge = chalList.Find(p => p.CharacterChallengerID == challengingChar.ID);
                    ResolveChallenge(curChallenge, challengingChar.ID, insultedChar.ID); // resolve the challenge immediately

                    if (insultedChar.Passion > 60 && insultedChar.Discretion < 30) // check for outrage/passion for challenged to raise the stakes
                    {
                        CreateChallenge(insultedChar.ID, challengingChar.ID, Random.Range(6, 15)); // if so, create new challenge!
                    }
                }

                secondCharacterInitialState.BetrayalLevel = 1f; // betrayed!
                conversationFlags += "[BETRAY]"; // add the flag
                challengingChar.Honor -= 10; // honor hit
            }

            if (secondCharacterInitialState.RelationshipState == Relationship.eRelationshipState.Inferior)
            {
                Challenge curChallenge;
                List<Challenge> chalList = gDataRef.ChallengeList.FindAll(p => p.CharacterChallengedID == insultedChar.ID);
                if (chalList.Exists(p => p.CharacterChallengerID == challengingChar.ID))
                {
                    curChallenge = chalList.Find(p => p.CharacterChallengerID == challengingChar.ID);
                    curChallenge.InfluenceStakedOnChallenge += Random.Range(2, 7);
                }
            }

            if (secondCharacterInitialState.RelationshipState == Relationship.eRelationshipState.Patron)
            {
                Challenge curChallenge;
                List<Challenge> chalList = gDataRef.ChallengeList.FindAll(p => p.CharacterChallengedID == insultedChar.ID);
                if (chalList.Exists(p => p.CharacterChallengerID == challengingChar.ID))
                {
                    curChallenge = chalList.Find(p => p.CharacterChallengerID == challengingChar.ID); // assign the winner to the challenge
                    if (curChallenge != null)
                    {
                        ResolveChallenge(curChallenge, challengingChar.ID, insultedChar.ID); // resolve the challenge immediately

                        if (insultedChar.Passion > 60 && insultedChar.Discretion < 30) // check for outrage/passion for challenged to raise the stakes
                        {
                            CreateChallenge(insultedChar.ID, challengingChar.ID, Random.Range(3, 10));
                        }
                    }             
                }

                secondCharacterInitialState.BetrayalLevel = 1f; // betrayed!
                conversationFlags += "[BETRAY]";
                actionResult += "BETRAYAL!";
            }

            if (secondCharacterInitialState.RelationshipState == Relationship.eRelationshipState.Protegee)
            {
                secondCharacterInitialState.BetrayalLevel = 1f; // betrayed!
                actionResult += "BETRAYAL!";
                conversationFlags += "[BETRAY]";
                challengingChar.Honor -= Random.Range(5, 15); // challenge character loses honor
                HelperFunctions.DataRetrivalFunctions.GetChallenge(activeChallengeID).InfluenceStakedOnChallenge += Random.Range(3, 7);
            }

            if (secondCharacterInitialState.RelationshipState == Relationship.eRelationshipState.Spouse)
            {
                secondCharacterInitialState.BetrayalLevel = 1f; // betrayed!
                actionResult += "BETRAYAL!";
                conversationFlags += "[BETRAY]";
                challengingChar.Honor -= Random.Range(5, 15); // challenge character loses honor
                HelperFunctions.DataRetrivalFunctions.GetChallenge(activeChallengeID).InfluenceStakedOnChallenge += Random.Range(3, 7);
            }

            if (activeChallengeID != "")
                secondCharacterInitialState.RelationshipState = Relationship.eRelationshipState.Challenged;

            string response = "NEW RELSTATE: " + secondCharacterInitialState.RelationshipState.ToString() + " " + actionResult + " " + ConversationEngine.GenerateResponse(insultedChar, aData, 0f, false, conversationFlags);
            return response;
        }
    }


}
