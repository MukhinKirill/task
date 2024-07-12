# Принцип S (Single Responsibility Principle)
class Calculator:
    def add(self, x, y):
        return x + y

# Принцип O (Open/Closed Principle)
class Shape:
    def area(self):
        pass

class Rectangle(Shape):
    def __init__(self, width, height):
        self.width = width
        self.height = height

    def area(self):
        return self.width * self.height

# Принцип L (Liskov Substitution Principle)
class Bird:
    def fly(self):
        pass

class Sparrow(Bird):
    def fly(self):
        print("Sparrow is flying")    

# Принцип I (Interface Segregation Principle)
class Printer:
    def print(self)
        pass

class Scanner:
    def scan(self)
        pass

class MultiFunctionDevice(Printer, Scanner):
    def print(self):
        print("Printing")

    def scan(self):
        print("Scanning")                                                            

