/**
 *  SmartHome
 *
 *  Copyright 2017 Melvina Tuscano
 *
 *  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 *  in compliance with the License. You may obtain a copy of the License at:
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
 *  on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License
 *  for the specific language governing permissions and limitations under the License.
 *
 */
definition(
 name: "SmartHome",
 namespace: "melvinatuscano",
 author: "Melvina Tuscano",
 description: "Bot framework test",
 category: "Family",
 iconUrl: "https://s3.amazonaws.com/smartapp-icons/Convenience/Cat-Convenience.png",
 iconX2Url: "https://s3.amazonaws.com/smartapp-icons/Convenience/Cat-Convenience@2x.png",
 iconX3Url: "https://s3.amazonaws.com/smartapp-icons/Convenience/Cat-Convenience@2x.png")


preferences {
 section("Allow external service to control these things...") {
  input "switches", "capability.switch", multiple: true, required: true
 }
}

def installed() {
 log.debug "Installed with settings: ${settings}"

 initialize()
}

def updated() {
 log.debug "Updated with settings: ${settings}"

 unsubscribe()
 initialize()
}

def initialize() {
 // TODO: subscribe to attributes, devices, locations, etc.
}

// TODO: implement event handlers

mappings {
 path("/switches") {
  action: [
   GET: "listSwitches"
  ]
 }
 path("/switches/:command") {
  action: [
   PUT: "updateSwitches"
  ]
 }
}

def listSwitches() {
 def resp = []
 switches.each {
  resp << [name: it.displayName, value: it.currentValue("switch")]
 }
 return resp
}

def updateSwitches() {
 //Gather operation detail
 def command = params.command
 def location = params.location
 def resp = [];

 //Perform operation
 switch (command) {
  case "off":
   switches.each {
    if (it.displayName == location || location == "all") {
     it.off();
     log.debug "$location is off..."
     resp << [name: it.displayName, value: it.currentValue("switch")];
     if (location != "all")
      return resp;
    }
   }
   if (resp.size() > 0)
    return resp
   else
    httpError(400, "Sorry We could not find location : $location")
   return

  case "on":
   switches.each {
    if (it.displayName == location || location == "all") {
     it.on();
     log.debug "$location is on..."
     resp << [name: it.displayName, value: it.currentValue("switch")];
    }
   }
   if (resp.size() > 0)
    return resp
   else
    httpError(400, "Sorry We could not find location : $location")
   return
  default:
   httpError(400, "Sorry $command is invalid.")
   return
 }
}
