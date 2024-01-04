using System;
using Random = UnityEngine.Random;

namespace FoundationFortune;

/// <summary>
/// the wall
/// </summary>
public static class WelcomeText
{
    public static void PrintMessage()
    {
        int randomNumber = Random.Range(1, 100001);
        DirectoryIterator.Log(randomNumber == 1
                ? @" 
            [Verse 1]
            What's with these homies dissin' my girl?
            Why do they gotta front?
            What did we ever do to these guys
            That made them so violent?
    
            [Pre-Chorus]
            Woo-hoo, but you know I'm yours
            Woo-hoo, and I know you're mine
            Woo-hoo, and that's for all of time
    
            [Chorus]
            Ooh wee ooh, I look just like Buddy Holly
            Oh oh, and you're Mary Tyler Moore
            I don't care what they say about us anyway
            I don't care 'bout that
    
            [Verse 2]
            Don't you ever fear, I'm always near
            I know that you need help
            Your tongue is twisted, your eyes are slit
            You need a guardian
    
            [Pre-Chorus]
            Woo-hoo, and you know I'm yours
            Woo-hoo, and I know you're mine
            Woo-hoo, and that's for all of time
    
            [Chorus]
            Ooh wee ooh, I look just like Buddy Holly
            Oh oh, and you're Mary Tyler Moore
            I don't care what they say about us anyway
            I don't care 'bout that
            I don't care 'bout that
    
            [Bridge]
            Bang, bang, knocking on the door
            Another big bang, get down on the floor
            Oh no, what do we do?
            Don't look now, but I lost my shoe
            I can't run and I can't kick
            What's a matter, babe, are you feelin' sick?
            What's a matter, what's a matter, what's a matter you?
            What's a matter, babe, are you feelin' blue? Oh-oh
    
            [Guitar Solo]
    
            [Pre-Chorus]
            And that's for all of time
            And that's for all of time
    
            [Chorus]
            Ooh wee ooh, I look just like Buddy Holly
            Oh oh, and you're Mary Tyler Moore
            I don't care what they say about us anyway
            I don't care 'bout that
            I don't care 'bout that
            I don't care 'bout that
            I don't care 'bout that

	   Welcome to Foundation Fortune lol"
                : @"
Welcome To   
              
                                                                                                                                       *%%%%%%%%*               
                                                                                                                                   %%%%%(      *%%%%%.          
                                                                                                                                 #%%%%              %%%%%        
      ______                      __      __  _                  ______           __                                            %%%..%%%,         %%%( %%%       
     / ____/___  __  ______  ____/ /___ _/ /_(_)___  ____       / ____/___  _____/ /___  ______  ___                           %%%     %%%%    #%%%     %%%      
    / /_  / __ \/ / / / __ \/ __  / __ `/ __/ / __ \/ __ \     / /_  / __ \/ ___/ __/ / / / __ \/ _ \                         ,%%%       /%%%%%%#       #%%/     
   / __/ / /_/ / /_/ / / / / /_/ / /_/ / /_/ / /_/ / / / /    / __/ / /_/ / /  / /_/ /_/ / / / /  __/                         *%%%        (%%%%%        /%%#     
  /_/    \____/\__,_/_/ /_/\__,_/\__,_/\__/_/\____/_/ /_/    /_/    \____/_/   \__/\__,_/_/ /_/\___/                           %%%      %%%%  #%%%      %%%      
                                                                                                                               *%%%  *%%%.       %%%#  %%%#      
                                                                                                                                 %%%%%%            #%%%%%        
                                                                                                                                  .%%%%*          .%%%%.         
                                                                                                                                      #%%%%%%%%%%%%#    
            

Lead Dev: DuoNineXcore
Devs: Nameless      
       "
            , ConsoleColor.DarkYellow);
    }  
}