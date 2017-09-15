namespace org.vr.rts
{

    abstract public class IRTSDefine
    {

        public enum Stack
        {
            ACTION_CONTINUE = 0X2,
            ACTION_BREAK = 0X4,
            ACTION_RETURN = 0X8,
            ACTION_HOLD = 0X10,
        }

        public enum Property
        {
            //  SCOPE_MASK = 0XF0,//
            //  MEMERY_MASK = 0XF00,//
            //  FUNC_MASK = 0XF000,//

            //  RTS_STATIC = 0X10,// static
            // // 修饰,表明定义或调用的变量和方法来自RTSEngine中的定义而非当前Section中的定义
            //  RTS_TEMP = 0x20,// temp
            GLOBAL = 0X10,// global 修饰 表示取值或者调用方法来自 RTSEngine，而非当前的stack空间
            CONST = 0x20,// const
            DECALRE = 0x40,// 声明
            //  RTS_COPY = 0x100,// copy 复制对象
            // 修饰,表明赋值或返回值时是给一个复制的对象(IRTSObj.makeACopy())而不是原本对象，这对于需要进行强转但又不希望改变原有值尤为重要
            //  RTS_DYNAMIC = 0x1000,// dynamic 修饰方法参数个数不定的方法，这些方法通常指外部定义的特殊方法

        }

        public enum Linker
        {

            PRIORITY_MASK = 0XFFFF,// 优先级位数

            TYPE = 0X1FFFC,// long bool...
            PROPERTY = 0X2FFFF, // 属性修饰符 用来修饰变量或者方法 static const ...
            BRACKET_FLOWER = 0X3FFFF, // {}
            BRACKET_FLOWER2 = 0X30000, // }
            BRACKET_SQUARE = 0X4FFFC,// []
            BRACKET_SQUARE2 = 0X50000,// ]
            BRACKET = 0X6FFFF,// ()
            BRACKET2 = 0X60000,// )

            VARIABLE = 0X7FFFD, // 变量
            FUNCTION = 0X8FFFD,// 方法
            FUNCTION_DEFINE = 0X9FFFD, // 方法定义
            DELETE = 0XA0003, // delete 用来删除变量定义或者方法定义

            COMMAND_H = 0XBFFFA,// command-high-priority
            COMMAND_L = 0XB0003,// command-low-priority

            IF = 0X10FFFC,// if
            THEN = 0x100000, // then
            ELSE = 0X12FFFC,// else
            FOR = 0X13FFFC,// for

            DOT = 0XAFFF1,// .

            BNOT = 0XBB000,// ~
            NOT = 0XCA000,// !

            SELFADD = 0X10F000,// ++
            SELFSUB = 0X11F000,// --

            MUL = 0X200B00,// *
            DIV = 0X210B00,// /
            MOD = 0X220B00,// %
            ADD = 0X230A00,// +
            SUB = 0X240A00,// -

            BITAND = 0X250800,// &
            BITOR = 0X260800,// |
            SHIFTL = 0X270700,// <<
            SHIFTR = 0X280700,// >>
            BITX = 0X290600,// ^

            LESS = 0X300400,// <
            MORE = 0X310400,// >
            NOTEQU = 0X320400,// !=
            EQUAL = 0X330400,// ==
            MOREQU = 0X340400,// >=
            LESSEQU = 0X350400,// <=

            AND = 0X400300,// && and
            OR = 0X410200,// || or
            XOR = 0X420400,// ~| xor

            EVALUATE = 0X500100,// = <<= >>= *= /= += -= %= &= |= ^=

            QUESTION = 0X600010,// ?

            COLON = unchecked((int)0XFFFA0005u),// :
            COMMA = unchecked((int)0XFFFB0004u),// ,
            STACK_ACT = unchecked((int)0XFFFC0003u),// return continue break
            SEMICOLON = unchecked((int)0XFFFF0001u),// ;
        }

        public enum Error
        {
            Compiling_InvalidLinkerL = 0x1,
            Compiling_InvalidLinkerR = 0x2,
            Compiling_InvalidLinkerL_rep = 0x4,
            Compiling_NullLinker = 0x8,
            Compiling_DenyLinker = 0x10,
            Runtime_DenyEvaluate = 0x20000,
            Runtime_NoFunctionDefine = 0x40000,
            Runtime_IndexOutOfBounds = 0x80000,
        }
    }

}