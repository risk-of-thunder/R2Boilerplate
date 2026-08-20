# What
This is intended to be used along side Sharesuite mod to provide blackisting item sources being shared instead of blacklisting items

# Why
To allow more gameplay decision making while still keeping the "together-ness" loadout feel of sharesuite:
- Advanced scrap macro with having 1 person holding all the scrap to manage cauldrons and printers for the team.
- Drifter's tinker giving 1 scrap to everyone in the lobby instead of eating it for 1 scrap for herself.
- Cooking balancing where you can't share non blacklisted crafted item for the team (ie having 1 firework and 1 sticky bomb in the run would give the whole X people lobby X Atgs each).
- (tbd) Halcyon seed balancing where you don't get X items for the whole X people lobby.
- (tbd) Recycling off of printed items on the floor instead of being immediately given to you.
- (tbd) Gorahk's Quality's printers don't get shafted by printercauldrondupefix.
- egg
# Who
Private mod. If you're reading this I maybe kinda know you.

# How
## Installation
- Go to Thunderstore
- Go to your Risk of Rain 2 game
- Go to your profile
- Go to settings
- Near the middle of the settings you will find the "Import Local Mod" button, click on that
- Select file
- Select the EggShare.zip, it should just work, you don't need to extract anything

## (Other's) mod config
- Go into Sharesuite's config (you will need to boot up the game with that mod once for the config file to generate)
- Find PrinterCauldronFix and set that to false
- Find ItemBlacklistDLC3+ and remove all the scrap from that list (if you wnat the benefit of Drifter's tinker giving 1 scrap to everyone)

## HOW how
Baby's first mod where I had to patch in an extra field into the UniquePickup struct to include data. not reccommended as a first mod experience