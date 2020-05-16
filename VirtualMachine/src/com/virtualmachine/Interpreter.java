package com.virtualmachine;
import java.util.Scanner;

public class Interpreter {

    public void interpret(Commands[] commands){
        int x = 0;

        for(int i = 0; i < commands.length; i++){
            Commands current = commands[i];

            switch(current){
                case INC_X:
                    x += 1;
                    break;

                case INPUT_X:
                    System.out.print("Input X: ");
                    Scanner scanner = new Scanner(System.in);
                    x = scanner.nextInt();
                    System.out.println();
                    break;

                case OUTPUT_X:
                    System.out.println("X is " + x);
                    break;

                default:
                    break;
            }
        }
    }
}
