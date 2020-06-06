Feature: Employees
	In order to manage Employees
	As a Manager
	I want to be able to list, create, edit and delete Employee records

@listEmployees
Scenario: List Employees
	Given I am on the Home Page
	When I click on Employees menu
	Then Employee List should be displayed