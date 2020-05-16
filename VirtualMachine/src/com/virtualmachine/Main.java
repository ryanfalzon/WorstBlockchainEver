package com.virtualmachine;

public class Main {

    public static void main(String[] args) {
	    Interpreter interpreter = new Interpreter();
	    interpreter.interpret(new Commands[]{ Commands.OUTPUT_X, Commands.INPUT_X, Commands.OUTPUT_X, Commands.INC_X, Commands.OUTPUT_X });
    }
}